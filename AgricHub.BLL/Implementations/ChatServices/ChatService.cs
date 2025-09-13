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

        public async Task<string> InitiateChatAsync(string consultantUserId, int? serviceId = null)
        {
            var userId = GetUserId();

            // Ensure the user is a customer
            var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId);
            if (customer == null)
                throw new UnauthorizedAccessException("Customer not found.");

            // Ensure the consultant exists
            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == consultantUserId);
            if (consultant == null)
                throw new KeyNotFoundException("Consultant not found.");

            // Validate the service if provided
            Service? service = null;
            if (serviceId.HasValue)
            {
                service = await _servicesRepo.GetSingleByAsync(s => s.Id == serviceId.Value,
                    include: q => q.Include(s => s.Business));
                if (service == null)
                    throw new KeyNotFoundException("Service not found.");

                // Ensure the service belongs to the consultant's business
                var business = await _businessRepo.GetSingleByAsync(b => b.Id == service.BusinessId && b.ConsultantId == consultant.Id);
                if (business == null)
                    throw new UnauthorizedAccessException("This service does not belong to the specified consultant.");
            }

            // Check for existing chat session
            var existingChat = await _chatSessionRepo.GetSingleByAsync(cs =>
                cs.CustomerId == customer.Id &&
                cs.ConsultantId == consultant.Id &&
                (serviceId.HasValue ? cs.ServiceId == serviceId : cs.ServiceId == null));

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
                    ServiceId = serviceId,
                    SendbirdChannelUrl = channelUrl,
                    CreatedAt = DateTime.UtcNow
                };

                await _chatSessionRepo.AddAsync(chatSession);
                await _unitOfWork.SaveChangesAsync();
            }

            // Send an initial system message with service context
            var message = service != null
                ? $"Chat initiated between {customer.FirstName} and {consultant.FirstName} regarding service: {service.ServiceName}."
                : $"Chat initiated between {customer.FirstName} and {consultant.FirstName}.";
            var serviceData = service != null
                ? new { ServiceId = service.Id, ServiceName = service.ServiceName, Price = service.Price }
                : null;
            await _sendbirdService.SendAdminMessageAsync(channelUrl, message, serviceData);


            return channelUrl;
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
         .Include(cs => cs.Customer)   // <-- add this
         .Include(cs => cs.Service));


            return _mapper.Map<IEnumerable<ChatSessionResponse>>(chatSessions);
        }



    }
}