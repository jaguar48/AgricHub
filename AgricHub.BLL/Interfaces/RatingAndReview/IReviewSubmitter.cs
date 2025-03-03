using System;
using AgricHub.Shared.DTO_s.Request.RatingAndReview;
using AgricHub.Shared.DTO_s.Response;


namespace AgricHub.BLL.Interfaces.RatingAndReview;

public interface IReviewSubmitter
{
    /// <summary>
    /// Submits a review for a consultant
    /// </summary>
    Task<OperationResult<int>> SubmitReviewAsync(int consultantId, int userId, 
        int rating, string? comment = null, int? consultationId = null);

    event Action<ReviewSubmittedEventArgs>? OnReviewSubmitted;
}
