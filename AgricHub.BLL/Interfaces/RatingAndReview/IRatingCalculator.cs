using System;

namespace AgricHub.BLL.Interfaces.RatingAndReview;

public interface IRatingCalculator
{
    /// <summary>
    /// Recalculates and caches rating aggregates for a consultant
    /// </summary>
    Task RecalculateRatingAggregatesAsync(int consultantId);
}
