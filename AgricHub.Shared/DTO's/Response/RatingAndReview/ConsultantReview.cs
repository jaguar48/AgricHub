namespace AgricHub.Shared.DTO_s.Response.RatingAndReview;

public record ConsultantReviewResponse(
    int Id,
    int ConsultantId,
    int UserId,
    int Rating,
    string? Comment,
    DateTime CreatedDate,  // Used for keyset pagination
    int? ConsultationId
);
