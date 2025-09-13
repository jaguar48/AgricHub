using AgricHub.DAL.Entities;
using AgricHub.Shared.DTO_s.Request;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgricHub.BLL.Interfaces.IRatingServices
{
    public interface IReviewService
    {
        Task<Review> AddReviewAsync(CreateReviewRequest request);
        Task<IEnumerable<Review>> GetReviewsForConsultantAsync(int consultantId);
        Task<double> GetAverageRatingForConsultantAsync(int consultantId);
    }
}
