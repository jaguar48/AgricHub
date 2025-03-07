using System;
using AgricHub.BLL.Interfaces.RatingAndReview;
using AgricHub.DAL.Context;
using AgricHub.DAL.Entities.Models.RatingAndReview;
using AgricHub.Shared.DTO_s.Response;
using AgricHub.Shared.DTO_s.Response.RatingAndReview;
using AgricHub.Shared.Enums.RatingAndReview;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgricHub.BLL.Implementations.RatingAndReview;

public class ReviewModerationService(AgricHubDbContext dbContext, ILogger<ReviewModerationService> logger) : IReviewModeration
{
    private readonly AgricHubDbContext _dbContext = dbContext;
    private readonly ILogger<ReviewModerationService> _logger = logger;

    public async Task<OperationResult<int>> FlagReviewAsync(int reviewId, string reportingUserId,
        ReportReason reason, string? details = null)
    {
        try
        {
            // Validate review exists
            if (!await _dbContext.ConsultantReviews.AnyAsync(r => r.Id == reviewId))
                return OperationResult<int>.NotFound("Review");

            // Prevent duplicate reporting
            var existingReport = await _dbContext.ReviewReports
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.ReportingUserId == reportingUserId);

            if (existingReport != null)
                return OperationResult<int>.Failure("You've already reported this review");

            var report = new ReviewReport
            {
                ReviewId = reviewId,
                ReportingUserId = reportingUserId,
                Reason = reason,
                Details = details,
                Status = ReportStatus.Pending,
                ReportedAt = DateTime.UtcNow,
                ResolvedAt = null,
                ResolvedByUserId = null
            };

            _dbContext.ReviewReports.Add(report);
            await _dbContext.SaveChangesAsync();

            return report.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flagging review {ReviewId}", reviewId);
            return OperationResult<int>.Failure("Failed to submit report");
        }
    }

    public async Task<PaginatedResult<ReviewReportResponse>> GetPendingReportsAsync(int pageSize = 50,
        DateTime? olderThan = null)
    {
        var query = _dbContext.ReviewReports
            .Where(r => r.Status == ReportStatus.Pending)
            .OrderBy(r => r.ReportedAt)
            .Select(r => new ReviewReportResponse(
                r.Id,
                r.ReviewId,
                r.ReportingUserId,
                r.Reason,
                r.Details,
                r.Status,
                r.ReportedAt,
                r.ResolvedAt,
                r.ResolvedByUserId
            ))
            .AsQueryable();

        if (olderThan.HasValue)
        {
            query = query.Where(r => r.ReportedAt < olderThan.Value);
        }

        var reports = await query
            .Take(pageSize + 1) // Fetch one extra to check for next page
            .ToListAsync();

        return new PaginatedResult<ReviewReportResponse>(
            Items: reports.Take(pageSize),
            HasNextPage: reports.Count > pageSize,
            NextPageKey: reports.Count > pageSize ? reports[pageSize - 1].ReportedAt : null
        );
    }

    public Task<ReviewReportResponse?> GetReportDetailsAsync(int reportId)
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult> UpdateReportStatusAsync(int reportId, ReportStatus newStatus,
        string moderatorUserId)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var report = await _dbContext.ReviewReports
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
                return OperationResult.NotFound("Report");

            if (report.Status == ReportStatus.ResolvedApproved ||
                report.Status == ReportStatus.ResolvedRejected)
            {
                return OperationResult.Failure("Cannot modify resolved reports");
            }

            report.Status = newStatus;
            report.ResolvedAt = DateTime.UtcNow;
            report.ResolvedByUserId = moderatorUserId;

            if (newStatus == ReportStatus.ResolvedApproved)
            {
                // Take action on the reviewed content
                var review = await _dbContext.ConsultantReviews
                    .FirstAsync(r => r.Id == report.ReviewId);

                review.IsHidden = true;
                _logger.LogInformation("Hid review {ReviewId} due to report approval", review.Id);
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return OperationResult.SuccessResult();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating report status {ReportId}", reportId);
            return OperationResult.Failure("Failed to update report status");
        }
    }
}


