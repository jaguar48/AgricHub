using System;
using AgricHub.BLL.Interfaces.RatingAndReview;
using AgricHub.DAL.Context;
using AgricHub.Shared.DTO_s.Request.RatingAndReview;
using AgricHub.Shared.DTO_s.Response;
using AgricHub.Shared.DTO_s.Response.RatingAndReview;
using AgricHub.Shared.Enums.RatingAndReview;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgricHub.BLL.Implementations.RatingAndReview;

public class ConsultantProfileService(
    AgricHubDbContext dbContext,
    ILogger<ConsultantProfileService> logger) : IReviewRetriever, IRatingCalculator
{
    private readonly AgricHubDbContext _dbContext = dbContext;
    private readonly ILogger<ConsultantProfileService> _logger = logger;

    public async Task RecalculateRatingAggregatesAsync(int consultantId)
    {
        try
        {
            var consultant = await _dbContext.Consultants
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == consultantId);

            if (consultant == null) return;

            var visibleReviews = consultant.Reviews?
                .Where(r => !r.IsHidden)
                .ToList() ?? [];

            consultant.TotalReviews = visibleReviews.Count;
            consultant.AverageRating = visibleReviews.Count != 0
                ? visibleReviews.Average(r => r.Rating)
                : 0;

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to recalculate aggregates for consultant {ConsultantId}", consultantId);
        }
    }

    public async Task<AggregatedRating> GetAggregatedRatingsAsync(int consultantId)
    {
        var consultant = await _dbContext.Consultants
            .FirstOrDefaultAsync(c => c.Id == consultantId);

        return consultant == null
            ? new AggregatedRating(0, 0, new Dictionary<int, int>())
            : new AggregatedRating(
                consultant.AverageRating ?? 0,
                consultant.TotalReviews ?? 0,
                await GetRatingDistributionAsync(consultantId));
    }

    private async Task<Dictionary<int, int>> GetRatingDistributionAsync(int consultantId)
    {
        return await _dbContext.ConsultantReviews
            .Where(r => r.ConsultantId == consultantId && !r.IsHidden)
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Rating, g => g.Count);
    }

    public async Task<PaginatedResult<ConsultantReviewResponse>> GetReviewsForConsultantAsync(
        int consultantId, 
        DateTime? afterDate = null, 
        int pageSize = 10, 
        ReviewSortOrder sortOrder = ReviewSortOrder.MostRecent)
    {
        try
        {
            var query = _dbContext.ConsultantReviews
                .Where(r => r.ConsultantId == consultantId && !r.IsHidden)
                .Include(r => r.User)
                .AsQueryable();

            // Apply sorting
            query = sortOrder switch
            {
                ReviewSortOrder.HighestRated => query.OrderByDescending(r => r.Rating),
                ReviewSortOrder.LowestRated => query.OrderBy(r => r.Rating),
                _ => query.OrderByDescending(r => r.CreatedDate)
            };

            // Keyset pagination
            if (afterDate.HasValue)
            {
                query = query.Where(r => r.CreatedDate < afterDate.Value);
            }

            var reviews = await query
                .Take(pageSize + 1)
                .ToListAsync();

            return new PaginatedResult<ConsultantReviewResponse>(
                Items: reviews.Take(pageSize),
                HasNextPage: reviews.Count > pageSize,
                NextPageKey: reviews.Count > pageSize ? reviews[pageSize - 1].CreatedDate : null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reviews for consultant {ConsultantId}", consultantId);
            return new PaginatedResult<ConsultantReviewResponse>(
                Items: [],
                HasNextPage: false,
                NextPageKey: null
            );
        }
    }
}
