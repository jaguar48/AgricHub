using AgricHub.BLL.Interfaces;
using AgricHub.BLL.Interfaces.ChatServices;
using AgricHub.BLL.Interfaces.IChatServices;
using AgricHub.Contracts;
using AgricHub.DAL.Entities;
using AgricHub.DAL.Entities.Models;
using AgricHub.Shared.DTO_s.Request;
using AgricHub.Shared.DTO_s.Response;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgricHub.BLL.Implementations
{
    public class ChatService : IChatService
    {
        private readonly IRepository<ChatSession> _chatSessionRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<Consultant> _consultantRepo;
        private readonly IRepository<Service> _servicesRepo;
        private readonly IRepository<CustomOffer> _customOfferRepo;
        private readonly IRepository<Business> _businessRepo;
        private readonly ISendbirdService _sendbirdService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public ChatService(
            IUnitOfWork unitOfWork,
            ISendbirdService sendbirdService,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _chatSessionRepo = _unitOfWork.GetRepository<ChatSession>();
            _customerRepo = _unitOfWork.GetRepository<Customer>();
            _consultantRepo = _unitOfWork.GetRepository<Consultant>();
            _servicesRepo = _unitOfWork.GetRepository<Service>();
            _customOfferRepo = _unitOfWork.GetRepository<CustomOffer>();
            _businessRepo = _unitOfWork.GetRepository<Business>();
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

        public async Task<string> InitiateChatAsync(InitiateChatRequest request)
        {
            var userId = GetUserId();

            // Ensure the user is a customer
            var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId);
            if (customer == null)
                throw new UnauthorizedAccessException("Customer not found.");

            // Ensure the consultant exists
            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == request.ConsultantUserId);
            if (consultant == null)
                throw new KeyNotFoundException("Consultant not found.");

            // Validate the service
            var service = await _servicesRepo.GetSingleByAsync(s => s.Id == request.ServiceId,
                include: q => q.Include(s => s.Business));
            if (service == null)
                throw new KeyNotFoundException("Service not found.");

            // Ensure the service belongs to the consultant's business
            var business = await _businessRepo.GetSingleByAsync(b => b.Id == service.BusinessId && b.ConsultantId == consultant.Id);
            if (business == null)
                throw new UnauthorizedAccessException("This service does not belong to the specified consultant.");

            // Check for existing chat session
            var existingChat = await _chatSessionRepo.GetSingleByAsync(cs =>
                cs.CustomerId == customer.Id &&
                cs.ConsultantId == consultant.Id &&
                cs.ServiceId == request.ServiceId);

            string channelUrl;
            if (existingChat != null)
            {
                channelUrl = existingChat.SendbirdChannelUrl;
            }
            else
            {
                // Create Sendbird users if they don't exist
                await _sendbirdService.EnsureSendbirdUserAsync(customer.UserId, $"{customer.FirstName} {customer.LastName}");
                await _sendbirdService.EnsureSendbirdUserAsync(consultant.UserId, $"{consultant.FirstName} {consultant.LastName}");

                // Create a distinct 1:1 Sendbird channel
                channelUrl = await _sendbirdService.CreateGroupChannelAsync(customer.UserId, consultant.UserId);

                // Create and save new chat session
                var chatSession = new ChatSession
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    ConsultantId = consultant.Id,
                    ServiceId = request.ServiceId,
                    SendbirdChannelUrl = channelUrl,
                    CreatedAt = DateTime.UtcNow
                };

                await _chatSessionRepo.AddAsync(chatSession);
                await _unitOfWork.SaveChangesAsync();
            }

            // Send an initial system message with service context
            var message = $"Chat initiated between {customer.FirstName} and {consultant.FirstName} regarding service: {service.ServiceName}.";
            var serviceData = new { ServiceId = service.Id, ServiceName = service.ServiceName, Price = service.Price };
            await _sendbirdService.SendAdminMessageAsync(channelUrl, message, serviceData);

            return channelUrl;
        }

        public async Task<CustomOfferResponse> CreateCustomOfferAsync(CustomOfferRequest request)
        {
            var userId = GetUserId();

            // Ensure the user is a consultant
            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == userId);
            if (consultant == null)
                throw new UnauthorizedAccessException("Consultant not found.");

            // Validate the chat session
            var chatSession = await _chatSessionRepo.GetSingleByAsync(cs => cs.Id == request.ChatSessionId,
                include: q => q.Include(cs => cs.Service).Include(cs => cs.Consultant));
            if (chatSession == null)
                throw new KeyNotFoundException("Chat session not found.");

            // Ensure the consultant owns the chat session
            if (chatSession.ConsultantId != consultant.Id)
                throw new UnauthorizedAccessException("You are not authorized to create an offer for this chat session.");

            // Validate the service
            var service = await _servicesRepo.GetSingleByAsync(s => s.Id == request.ServiceId,
                include: q => q.Include(s => s.Business));
            if (service == null)
                throw new KeyNotFoundException("Service not found.");

            // Ensure the service belongs to the consultant's business
            var business = await _businessRepo.GetSingleByAsync(b => b.Id == service.BusinessId && b.ConsultantId == consultant.Id);
            if (business == null)
                throw new UnauthorizedAccessException("This service does not belong to the specified consultant.");

            // Create custom offer
            var customOffer = _mapper.Map<CustomOffer>(request);
            customOffer.Status = "Pending";
            customOffer.CreatedAt = DateTime.UtcNow;

            await _customOfferRepo.AddAsync(customOffer);
            await _unitOfWork.SaveChangesAsync();

            // Send admin message to chat channel
            var message = $"Custom offer created for service: {service.ServiceName}. Price: {customOffer.Price}. Description: {customOffer.Description}. Onsite: {customOffer.IncludesOnsiteVisit}.";
            var offerData = new { OfferId = customOffer.Id, ServiceId = service.Id, ServiceName = service.ServiceName, Price = customOffer.Price, Description = customOffer.Description, IncludesOnsiteVisit = customOffer.IncludesOnsiteVisit };
            await _sendbirdService.SendAdminMessageAsync(chatSession.SendbirdChannelUrl, message, offerData);

            return _mapper.Map<CustomOfferResponse>(customOffer);
        }

        public async Task<CustomOfferResponse> AcceptCustomOfferAsync(Guid offerId)
        {
            var userId = GetUserId();

            // Ensure the user is a customer
            var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId);
            if (customer == null)
                throw new UnauthorizedAccessException("Customer not found.");

            // Validate the offer
            var customOffer = await _customOfferRepo.GetSingleByAsync(co => co.Id == offerId,
                include: q => q.Include(co => co.ChatSession).ThenInclude(cs => cs.Customer));
            if (customOffer == null)
                throw new KeyNotFoundException("Custom offer not found.");

            // Ensure the customer is part of the chat session
            if (customOffer.ChatSession.CustomerId != customer.Id)
                throw new UnauthorizedAccessException("You are not authorized to accept this offer.");

            if (customOffer.Status != "Pending")
                throw new InvalidOperationException("Only pending offers can be accepted.");

            customOffer.Status = "Accepted";
            customOffer.AcceptedAt = DateTime.UtcNow;

            _customOfferRepo.Update(customOffer);
            await _unitOfWork.SaveChangesAsync();

            // Send admin message
            await _sendbirdService.SendAdminMessageAsync(customOffer.ChatSession.SendbirdChannelUrl,
                $"Custom offer accepted for service: {customOffer.Service.ServiceName}. Price: {customOffer.Price}.");

            return _mapper.Map<CustomOfferResponse>(customOffer);
        }

        public async Task<CustomOfferResponse> RejectCustomOfferAsync(Guid offerId, string reason)
        {
            var userId = GetUserId();

            // Ensure the user is a customer
            var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId);
            if (customer == null)
                throw new UnauthorizedAccessException("Customer not found.");

            // Validate the offer
            var customOffer = await _customOfferRepo.GetSingleByAsync(co => co.Id == offerId,
                include: q => q.Include(co => co.ChatSession).ThenInclude(cs => cs.Customer));
            if (customOffer == null)
                throw new KeyNotFoundException("Custom offer not found.");

            // Ensure the customer is part of the chat session
            if (customOffer.ChatSession.CustomerId != customer.Id)
                throw new UnauthorizedAccessException("You are not authorized to reject this offer.");

            if (customOffer.Status != "Pending")
                throw new InvalidOperationException("Only pending offers can be rejected.");

            customOffer.Status = "Rejected";

            _customOfferRepo.Update(customOffer);
            await _unitOfWork.SaveChangesAsync();

            // Send admin message
            await _sendbirdService.SendAdminMessageAsync(customOffer.ChatSession.SendbirdChannelUrl,
                $"Custom offer rejected for service: {customOffer.Service.ServiceName}. Reason: {reason}.");

            return _mapper.Map<CustomOfferResponse>(customOffer);
        }

        public async Task<IEnumerable<ChatSessionResponse>> GetMyChatsAsync()
        {
            var userId = GetUserId();
            var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId)
                          ?? throw new UnauthorizedAccessException("Customer not found.");

            var chatSessions = await _chatSessionRepo.GetAllAsync(
                cs => cs.CustomerId == customer.Id,
                include: q => q
                    .Include(cs => cs.Customer)
                    .Include(cs => cs.Consultant)
                    .Include(cs => cs.Service));

            return _mapper.Map<IEnumerable<ChatSessionResponse>>(chatSessions);
        }

        public async Task<IEnumerable<ChatSessionResponse>> GetConsultantChatsAsync()
        {
            var userId = GetUserId();
            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == userId)
                           ?? throw new UnauthorizedAccessException("Consultant not found.");

            var chatSessions = await _chatSessionRepo.GetAllAsync(
                cs => cs.ConsultantId == consultant.Id,
                include: q => q
                    .Include(cs => cs.Customer)
                    .Include(cs => cs.Consultant)
                    .Include(cs => cs.Service));

            return _mapper.Map<IEnumerable<ChatSessionResponse>>(chatSessions);
        }
    }
}