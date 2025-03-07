using System;
using AgricHub.BLL.Interfaces.RatingAndReview;
using AgricHub.DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace AgricHub.BLL.Implementations.RatingAndReview;

public class ConsultationValidator(AgricHubDbContext dbContext) : IConsultationValidator
{
    private readonly AgricHubDbContext _dbContext = dbContext;

    public async Task<bool> HasCompletedConsultationAsync(string userId, int consultantId)
    {
        return await _dbContext.Consultations
            .AnyAsync(c => c.UserId == userId
                && c.ConsultantId == consultantId
                && c.IsCompleted
                && c.EndDate < DateTime.UtcNow);
    }
}