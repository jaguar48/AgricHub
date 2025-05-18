using AgricHub.BLL.Interfaces.ChatServices;
using AgricHub.BLL.Interfaces.IBusinessServices;
using AgricHub.Contracts;
using AgricHub.DAL.Entities;
using AgricHub.DAL.Entities.Models;
using AgricHub.Shared.DTO_s.Request;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AgricHub.BLL.Implementations.BusinessServices
{
    public class ConsultationService : IConsultationService
    {
        private readonly IRepository<Consultation> _consultationRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<Consultant> _consultantRepo;
        private readonly ISendbirdService _sendbirdService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ConsultationService(
            IUnitOfWork unitOfWork,
            ISendbirdService sendbirdService,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _consultationRepo = _unitOfWork.GetRepository<Consultation>();
            _customerRepo = _unitOfWork.GetRepository<Customer>();
            _consultantRepo = _unitOfWork.GetRepository<Consultant>();
            _sendbirdService = sendbirdService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Consultation> BookConsultationAsync(ConsultationBookingRequest dto)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User is not authenticated.");

            var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId);
            if (customer == null)
                throw new UnauthorizedAccessException("Agropreneur not found.");

            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == dto.ConsultantId);
            if (consultant == null)
                throw new KeyNotFoundException("Consultant not found.");

            var isSlotTaken = await _consultationRepo.AnyAsync(c =>
                c.ConsultantId == consultant.Id &&
                c.ScheduledAt == dto.ScheduledAt);
            if (isSlotTaken)
                throw new InvalidOperationException("This time slot is already booked with the consultant.");

            // 🔐 Ensure both parties exist on Sendbird
            await _sendbirdService.CreateSendbirdUserAsync(); // Logged-in agropreneur
            await _sendbirdService.CreateSendbirdUserAsync(dto.ConsultantId, consultant.FirstName ); // Consultant

            var consultation = new Consultation
            {
                CustomerId = customer.Id,
                ConsultantId = consultant.Id,
                ScheduledAt = dto.ScheduledAt
            };

            await _consultationRepo.AddAsync(consultation);
            await _unitOfWork.SaveChangesAsync();

            var channelUrl = await _sendbirdService.CreateGroupChannelAsync(userId, dto.ConsultantId);
            consultation.SendbirdChannelUrl = channelUrl;

            await _unitOfWork.SaveChangesAsync();

            return consultation;
        }


    }
}
