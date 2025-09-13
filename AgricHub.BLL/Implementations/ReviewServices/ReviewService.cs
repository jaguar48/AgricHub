
using AgricHub.BLL.Interfaces.IRatingServices;
using AgricHub.Contracts;
using AgricHub.DAL.Entities;
using AgricHub.DAL.Entities.Models;
using AgricHub.Shared.DTO_s.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgricHub.BLL.Implementations.ReviewServices
{
    public class ReviewService : IReviewService
    {
        private readonly IRepository<Review> _reviewRepo;
        private readonly IRepository<Consultation> _consultationRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReviewService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;

            _reviewRepo = _unitOfWork.GetRepository<Review>();
            _consultationRepo = _unitOfWork.GetRepository<Consultation>();
            _customerRepo = _unitOfWork.GetRepository<Customer>();
        }

        public async Task<Review> AddReviewAsync(CreateReviewRequest request)
        {
            // 🔐 Check logged-in user
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated.");

            var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId);
            if (customer == null) throw new UnauthorizedAccessException("Customer not found.");

            var consultation = await _consultationRepo.GetByIdAsync(request.ConsultationId);
            if (consultation == null) throw new KeyNotFoundException("Consultation not found.");

            // ✅ Ensure only the customer who booked can review
            if (consultation.CustomerId != customer.Id)
                throw new UnauthorizedAccessException("You can only review your own consultations.");

            // ✅ Ensure consultation is completed
            if (consultation.Status != "Completed")
                throw new InvalidOperationException("Only completed consultations can be reviewed.");

            // ❌ Prevent multiple reviews per consultation
            var existing = await _reviewRepo.GetSingleByAsync(r => r.ConsultationId == consultation.Id);
            if (existing != null) throw new InvalidOperationException("This consultation already has a review.");

            var review = new Review
            {
                ConsultationId = consultation.Id,
                CustomerId = consultation.CustomerId,
                ConsultantId = consultation.ConsultantId,
                ServiceId = consultation.ServiceId,
                Comment = request.Comment
            };

            await _reviewRepo.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            return review;
        }

        public async Task<IEnumerable<Review>> GetReviewsForConsultantAsync(int consultantId)
        {
            return await _reviewRepo.GetAllAsync(
                r => r.ConsultantId == consultantId,
                include: q => q.Include(r => r.Customer).Include(r => r.Service)
            );
        }

        public async Task<double> GetAverageRatingForConsultantAsync(int consultantId)
        {
            var reviews = await _reviewRepo.GetAllAsync(r => r.ConsultantId == consultantId);

            if (!reviews.Any())
                return 0.0;
            return reviews.Average(r => r.Rating);
        }

    }
}
