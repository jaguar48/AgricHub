// ReviewService.cs
using System;
using AgricHub.BLL.Interfaces.RatingAndReview;
using AgricHub.DAL.Context;
using AgricHub.DAL.Entities.Models.RatingAndReview;
using AgricHub.Shared.DTO_s.Request.RatingAndReview;
using AgricHub.Shared.DTO_s.Response;
using AgricHub.Shared.DTO_s.Response.RatingAndReview;
using AgricHub.Shared.Enums.RatingAndReview;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgricHub.BLL.Implementations.RatingAndReview
{
    public class ReviewService(
        AgricHubDbContext dbContext,
        IConsultationValidator validator,
        ILogger<ReviewService> logger,
        IRatingCalculator ratingCalculator) : IReviewSubmitter, IReviewRetriever
    {
        private readonly AgricHubDbContext _dbContext = dbContext;
        private readonly IConsultationValidator _validator = validator;
        private readonly ILogger<ReviewService> _logger = logger;
        private readonly IRatingCalculator _ratingCalculator = ratingCalculator;

        public event Action<ReviewSubmittedEventArgs>? OnReviewSubmitted;

        public async Task<OperationResult<int>> SubmitReviewAsync(int consultantId, int userId,
            int rating, string? comment = null, int? consultationId = null)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            
            try
            {
                // Validate rating range
                if (rating < 1 || rating > 5)
                    return OperationResult<int>.ValidationFailed("Rating must be between 1-5");

                // Check consultation validity
                if (!await _validator.HasCompletedConsultationAsync(userId, consultantId))
                    return OperationResult<int>.Forbidden();

                // Check for existing review for this consultation
                if (consultationId.HasValue && await _dbContext.ConsultantReviews
                    .AnyAsync(r => r.ConsultationId == consultationId))
                {
                    return OperationResult<int>.ValidationFailed(
                        "You've already reviewed this consultation");
                }

                var newReview = new ConsultantReview
                {
                    ConsultantId = consultantId,
                    UserId = userId,
                    Rating = rating,
                    Comment = comment,
                    CreatedDate = DateTime.UtcNow,
                    ConsultationId = consultationId,
                    IsHidden = false
                };

                await _dbContext.ConsultantReviews.AddAsync(newReview);
                await _dbContext.SaveChangesAsync();

                // Update aggregates
                await _ratingCalculator.RecalculateRatingAggregatesAsync(consultantId);

                // Trigger event
                OnReviewSubmitted?.Invoke(new ReviewSubmittedEventArgs(
                    newReview.Id,
                    consultantId,
                    DateTime.UtcNow
                ));

                await transaction.CommitAsync();
                return newReview.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to submit review for consultant {ConsultantId}", consultantId);
                return OperationResult<int>.Failure("Failed to submit review");
            }
        }

        public async Task<PaginatedResult<ConsultantReviewResponse>> GetReviewsForConsultantAsync(
            int consultantId, DateTime? afterDate = null, int pageSize = 10,
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

                // Apply keyset pagination
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
                _logger.LogError(ex, "Failed to retrieve reviews for consultant {ConsultantId}", consultantId);
                return new PaginatedResult<ConsultantReviewResponse>(
                    Items: [],
                    HasNextPage: false,
                    NextPageKey: null
                );
            }
        }

        public async Task<AggregatedRating> GetAggregatedRatingsAsync(int consultantId)
        {
            var consultant = await _dbContext.Consultants
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == consultantId);

            if (consultant == null)
                return new AggregatedRating(0, 0, new Dictionary<int, int>());

            return new AggregatedRating(
                consultant.AverageRating ?? 0,
                consultant.TotalReviews ?? 0,
                consultant.Reviews?
                    .Where(r => !r.IsHidden)
                    .GroupBy(r => r.Rating)
                    .ToDictionary(g => g.Key, g => g.Count()) ?? []
            );
        }
    }
}