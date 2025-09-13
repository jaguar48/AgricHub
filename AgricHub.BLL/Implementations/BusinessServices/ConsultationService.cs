using AgricHub.BLL.Interfaces.ChatServices;
using AgricHub.BLL.Interfaces.IBusinessServices;
using AgricHub.Contracts;
using AgricHub.DAL.Entities;
using AgricHub.DAL.Entities.Models;
using AgricHub.Shared.DTO_s.Request;
using AgricHub.Shared.DTO_s.Response;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgricHub.BLL.Implementations.BusinessServices
{
    public class ConsultationService : IConsultationService
    {
        private readonly IRepository<Consultation> _consultationRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<Consultant> _consultantRepo;
        private readonly IRepository<Service> _servicesRepo;
        private readonly IRepository<Business> _businessRepo;
        private readonly IRepository<ChatSession> _chatSessionRepo;
        private readonly ISendbirdService _sendbirdService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public ConsultationService(
            IUnitOfWork unitOfWork,
            ISendbirdService sendbirdService,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _consultationRepo = _unitOfWork.GetRepository<Consultation>();
            _customerRepo = _unitOfWork.GetRepository<Customer>();
            _consultantRepo = _unitOfWork.GetRepository<Consultant>();
            _servicesRepo = _unitOfWork.GetRepository<Service>();
            _businessRepo = _unitOfWork.GetRepository<Business>();
            _chatSessionRepo = _unitOfWork.GetRepository<ChatSession>();
            _sendbirdService = sendbirdService;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        private string GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User is not authenticated.");
            return userId;
        }

        private async Task EnsureConsultantOwnershipAsync(Consultation consultation, string userId)
        {
            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == userId);
            if (consultant == null || consultant.Id != consultation.ConsultantId)
                throw new UnauthorizedAccessException("You are not authorized to manage this consultation.");
        }

        private async Task EnsureCustomerOrConsultantAsync(Consultation consultation, string userId)
        {
            var isCustomer = consultation.Customer?.UserId == userId;
            var isConsultant = consultation.Consultant?.UserId == userId;

            if (!isCustomer && !isConsultant)
                throw new UnauthorizedAccessException("You are not authorized to perform this action.");
        }

        public async Task<Consultation> BookConsultationAsync(ConsultationBookingRequest dto)
        {
            var userId = GetUserId();

            // Ensure the user is a customer
            var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId);
            if (customer == null)
                throw new UnauthorizedAccessException("Customer not found.");

            // Ensure the consultant exists
            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == dto.ConsultantId);
            if (consultant == null)
                throw new KeyNotFoundException("Consultant not found.");

            // Validate the service
            var service = await _servicesRepo.GetSingleByAsync(s => s.Id == dto.ServiceId,
                include: q => q.Include(s => s.Business));
            if (service == null)
                throw new KeyNotFoundException("Service not found.");

            // Ensure the service belongs to the consultant's business
            var business = await _businessRepo.GetSingleByAsync(b => b.Id == service.BusinessId && b.ConsultantId == consultant.Id);
            if (business == null)
                throw new UnauthorizedAccessException("This service does not belong to the specified consultant.");

            // Check if the slot is taken
            var isSlotTaken = await _consultationRepo.AnyAsync(c =>
                c.ConsultantId == consultant.Id &&
                c.ScheduledAt == dto.ScheduledAt);
            if (isSlotTaken)
                throw new InvalidOperationException("This time slot is already booked.");

            // Map DTO to Consultation
            var consultation = _mapper.Map<Consultation>(dto);
            consultation.CustomerId = customer.Id;
            consultation.ConsultantId = consultant.Id;
            consultation.ServiceId = dto.ServiceId;

            // Check for existing chat session
            var existingChat = await _chatSessionRepo.GetSingleByAsync(cs =>
                cs.CustomerId == customer.Id &&
                cs.ConsultantId == consultant.Id &&
                cs.ServiceId == dto.ServiceId);

            if (existingChat != null)
            {
                consultation.SendbirdChannelUrl = existingChat.SendbirdChannelUrl;
            }
            else
            {
                // Create Sendbird users if they don't exist
                await _sendbirdService.CreateSendbirdUserAsync(customer.UserId, $"{customer.FirstName} {customer.LastName}");
                await _sendbirdService.CreateSendbirdUserAsync(consultant.UserId, $"{consultant.FirstName} {consultant.LastName}");

                // Create a new channel
                var channelUrl = await _sendbirdService.CreateGroupChannelAsync(customer.UserId, consultant.UserId);
                consultation.SendbirdChannelUrl = channelUrl;

                // Save chat session
                var chatSession = new ChatSession
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    ConsultantId = consultant.Id,
                    ServiceId = dto.ServiceId,
                    SendbirdChannelUrl = channelUrl,
                    CreatedAt = DateTime.UtcNow
                };
                await _chatSessionRepo.AddAsync(chatSession);
            }

            await _consultationRepo.AddAsync(consultation);
            await _unitOfWork.SaveChangesAsync();

            // Send initial message
            if (!string.IsNullOrWhiteSpace(dto.Notes))
            {
                await _sendbirdService.SendMessageAsync(consultation.SendbirdChannelUrl, customer.UserId,
                    $"Booking request for service: {service.ServiceName}. Notes: {dto.Notes}");
            }
            else
            {
                await _sendbirdService.SendMessageAsync(consultation.SendbirdChannelUrl, "system",
                    $"Booking request submitted for service: {service.ServiceName}.", true);
            }

            return _mapper.Map<Consultation>(consultation);
        }

        // Other methods (ApproveConsultationAsync, RejectConsultationAsync, etc.) remain unchanged
        public async Task<ConsultationResponse> ApproveConsultationAsync(Guid consultationId, string? notes = null)
        {
            var userId = GetUserId();
            var consultation = await _consultationRepo.GetByIdAsync(consultationId)
                ?? throw new KeyNotFoundException("Consultation not found.");

            await EnsureConsultantOwnershipAsync(consultation, userId);

            if (consultation.Status != "Pending")
                throw new InvalidOperationException("Only pending consultations can be approved.");

            consultation.Status = "Approved";

            if (string.IsNullOrEmpty(consultation.SendbirdChannelUrl))
            {
                var customer = await _customerRepo.GetByIdAsync(consultation.CustomerId);
                var consultant = await _consultantRepo.GetByIdAsync(consultation.ConsultantId);

                await _sendbirdService.CreateSendbirdUserAsync(customer.UserId, $"{customer.FirstName} {customer.LastName}");
                await _sendbirdService.CreateSendbirdUserAsync(consultant.UserId, $"{consultant.FirstName} {consultant.LastName}");

                var channelUrl = await _sendbirdService.CreateGroupChannelAsync(customer.UserId, consultant.UserId);
                consultation.SendbirdChannelUrl = channelUrl;
            }

            await _unitOfWork.SaveChangesAsync();

            await _sendbirdService.SendMessageAsync(consultation.SendbirdChannelUrl, "system", "✅ Consultation approved.", true);

            return _mapper.Map<ConsultationResponse>(consultation);
        }

        public async Task<ConsultationResponse> RejectConsultationAsync(Guid consultationId, string reason)
        {
            var userId = GetUserId();
            var consultation = await _consultationRepo.GetByIdAsync(consultationId)
                ?? throw new KeyNotFoundException("Consultation not found.");

            await EnsureConsultantOwnershipAsync(consultation, userId);

            if (consultation.Status != "Pending")
                throw new InvalidOperationException("Only pending consultations can be rejected.");

            consultation.Status = "Rejected";
            await _unitOfWork.SaveChangesAsync();

            await _sendbirdService.SendMessageAsync(consultation.SendbirdChannelUrl, "system", $"❌ Consultation rejected. Reason: {reason}", true);

            return _mapper.Map<ConsultationResponse>(consultation);
        }

        public async Task<ConsultationResponse> StartConsultationAsync(Guid consultationId)
        {
            var userId = GetUserId();
            var consultation = await _consultationRepo.GetByIdAsync(consultationId)
                ?? throw new KeyNotFoundException("Consultation not found.");

            await EnsureConsultantOwnershipAsync(consultation, userId);

            if (consultation.Status != "Approved")
                throw new InvalidOperationException("Only approved consultations can be started.");

            consultation.Status = "In Progress";
            await _unitOfWork.SaveChangesAsync();

            await _sendbirdService.SendMessageAsync(consultation.SendbirdChannelUrl, "system", "🚀 Consultation started.", true);

            return _mapper.Map<ConsultationResponse>(consultation);
        }

        public async Task<ConsultationResponse> CompleteConsultationAsync(Guid consultationId)
        {
            var userId = GetUserId();
            var consultation = await _consultationRepo.GetByIdAsync(consultationId)
                ?? throw new KeyNotFoundException("Consultation not found.");

            await EnsureConsultantOwnershipAsync(consultation, userId);

            if (consultation.Status != "In Progress")
                throw new InvalidOperationException("Only in-progress consultations can be completed.");

            consultation.Status = "Completed";
            await _unitOfWork.SaveChangesAsync();

            await _sendbirdService.SendMessageAsync(consultation.SendbirdChannelUrl, "system", "✅ Consultation completed.", true);

            return _mapper.Map<ConsultationResponse>(consultation);
        }

        public async Task<ConsultationResponse> CancelConsultationAsync(Guid consultationId, string? reason = null)
        {
            var userId = GetUserId();
            var consultation = await _consultationRepo.GetByIdAsync(consultationId)
                ?? throw new KeyNotFoundException("Consultation not found.");

            await EnsureCustomerOrConsultantAsync(consultation, userId);

            if (consultation.Status == "Completed" || consultation.Status == "Rejected")
                throw new InvalidOperationException("Cannot cancel a completed or rejected consultation.");

            consultation.Status = "Cancelled";
            await _unitOfWork.SaveChangesAsync();

            var msg = string.IsNullOrEmpty(reason)
                ? "⚠️ Consultation cancelled."
                : $"⚠️ Consultation cancelled. Reason: {reason}";
            await _sendbirdService.SendMessageAsync(consultation.SendbirdChannelUrl, "system", msg, true);

            return _mapper.Map<ConsultationResponse>(consultation);
        }

        public async Task<IEnumerable<ConsultationResponse>> GetMyConsultationsAsync()
        {
            var userId = GetUserId();
            var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId)
                          ?? throw new UnauthorizedAccessException("Customer not found.");

            var consultations = await _consultationRepo.GetAllAsync(c => c.CustomerId == customer.Id,
                include: q => q.Include(c => c.Consultant).Include(c => c.Service));

            return _mapper.Map<IEnumerable<ConsultationResponse>>(consultations);
        }

        public async Task<IEnumerable<ConsultationResponse>> GetConsultantConsultationsAsync()
        {
            var userId = GetUserId();
            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == userId)
                           ?? throw new UnauthorizedAccessException("Consultant not found.");

            var consultations = await _consultationRepo.GetAllAsync(c => c.ConsultantId == consultant.Id,
                include: q => q.Include(c => c.Customer).Include(c => c.Service));

            return _mapper.Map<IEnumerable<ConsultationResponse>>(consultations);
        }
    }
}