using System;
using AgricHub.BLL.Interfaces.RatingAndReview;
using AgricHub.DAL.Context;

namespace AgricHub.BLL.Implementations.RatingAndReview;

public class ConsultationValidator(AgricHubDbContext dbContext) : IConsultationValidator
{
    private readonly AgricHubDbContext _dbContext = dbContext;

    public async Task<bool> HasCompletedConsultationAsync(int userId, int consultantId)
    {
        return await _dbContext.Consultations
            .AnyAsync(c => c.UserId == userId
                && c.ConsultantId == consultantId
                && c.IsCompleted
                && c.EndDate < DateTime.UtcNow);
    }
}