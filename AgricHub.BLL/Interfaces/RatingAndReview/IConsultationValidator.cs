using System;

namespace AgricHub.BLL.Interfaces.RatingAndReview;

public interface IConsultationValidator
{
    /// <summary>
    /// Verifies if user had a consultation with consultant
    /// </summary>
    Task<bool> HasCompletedConsultationAsync(int userId, int consultantId);
}
