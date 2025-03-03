using System;
using AgricHub.Shared.DTO_s.Request.RatingAndReview;
using AgricHub.Shared.DTO_s.Response;
using AgricHub.Shared.DTO_s.Response.RatingAndReview;
using AgricHub.Shared.Enums.RatingAndReview;

namespace AgricHub.BLL.Interfaces.RatingAndReview;

public interface IReviewRetriever
{
    /// <summary>
    /// Gets reviews using keyset pagination
    /// </summary>
    /// <param name="afterDate">Exclusive starting point for pagination (UTC)</param>
    /// <param name="pageSize">Number of items to return</param>
    Task<PaginatedResult<ConsultantReviewResponse>> GetReviewsForConsultantAsync(int consultantId, 
        DateTime? afterDate = null, int pageSize = 10,
        ReviewSortOrder sortOrder = ReviewSortOrder.MostRecent);

    Task<AggregatedRating> GetAggregatedRatingsAsync(int consultantId);
}