using AgricHub.BLL.Interfaces.IAgrichub_Services;
using AgricHub.BLL.Interfaces.IUserServices;
using AgricHub.Contracts;
using AgricHub.DAL.Entities;
using AgricHub.DAL.Entities.Models;
using AgricHub.Shared.DTO_s.Request;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Security.Claims;

namespace AgricHub.BLL.Implementations.AgrichubServices
{

    public class BusinessForService : IBusinessForService
    {

        private readonly IRepository<Service> _servicesRepo;
        private readonly IRepository<Business> _businessRepo;
        private readonly IRepository<Consultant> _consultantRepo;
        private readonly IUnitOfWork _unitOfWork;
        /*private readonly ILoggerManager _logger;*/
        private readonly IUserServices _userServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;


        public BusinessForService(IMapper mapper, IHttpContextAccessor httpContextAccessor, IAuthService authService, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;

            _businessRepo = _unitOfWork.GetRepository<Business>();
            _consultantRepo = _unitOfWork.GetRepository<Consultant>();
            _servicesRepo = _unitOfWork.GetRepository<Service>();

        }

        public async Task<string> AddService(CreateServiceRequest serviceRequest)
        {
            if (serviceRequest.File == null || serviceRequest.File.Length == 0)
            {
                throw new Exception("Image file is required.");
            }

            
            if (serviceRequest.File.Length > 5 * 1024 * 1024) 
            {
                throw new Exception("File size exceeds the 5MB limit.");
            }

          
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(serviceRequest.File.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new Exception("Invalid file type. Only JPG, JPEG, and PNG are allowed.");
            }

            var folderName = Path.Combine("Resources", "Images");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            if (!Directory.Exists(pathToSave))
            {
                Directory.CreateDirectory(pathToSave);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(serviceRequest.File.FileName);
            var fullPath = Path.Combine(pathToSave, fileName);
            var dbPath = Path.Combine(folderName, fileName).Replace('\\', '/');

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await serviceRequest.File.CopyToAsync(stream);
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                throw new Exception("User not found.");
            }

           
            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == userId);
            if (consultant == null)
            {
                throw new UnauthorizedAccessException("No consultant found for this user.");
            }

            
            var business = await _businessRepo.GetSingleByAsync(b => b.Id == serviceRequest.BusinessId && b.ConsultantId == consultant.Id);
            if (business == null)
            {
                throw new UnauthorizedAccessException("You are not authorized to add a service to this business.");
            }

           
            if (!business.IsVerified)
            {
                throw new Exception("This business is not verified. Please verify it before adding services.");
            }

           
            var service = _mapper.Map<Service>(serviceRequest);

           
            service.BusinessId = business.Id;
            service.Business = business; 

           
            service.DateAdded = DateTime.UtcNow;

           
            await _servicesRepo.AddAsync(service);
            await _unitOfWork.SaveChangesAsync();

            
            return JsonConvert.SerializeObject(new { success = true, message = "Service created successfully." });
        }

    }
}