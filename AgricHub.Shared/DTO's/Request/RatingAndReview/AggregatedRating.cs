namespace AgricHub.Shared.DTO_s.Request.RatingAndReview;

public record AggregatedRating(
    double AverageRating,
    int TotalReviews,
    IReadOnlyDictionary<int, int> RatingDistribution
);
