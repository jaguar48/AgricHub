namespace AgricHub.Shared.DTO_s.Request.RatingAndReview;


public record ReviewSubmittedEventArgs(
    int ReviewId, 
    int ConsultantId, 
    DateTime SubmittedAt
);
