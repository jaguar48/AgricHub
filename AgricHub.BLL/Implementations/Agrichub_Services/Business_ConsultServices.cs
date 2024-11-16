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
    public class BusinessConsultService : IBusiness_ConsultServices
    {
        private readonly IRepository<Consultant> _consultantRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<Business> _businessRepo;
        private readonly IUnitOfWork _unitOfWork;
        /*private readonly ILoggerManager _logger;*/
        private readonly IUserServices _userServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;


        public BusinessConsultService(IMapper mapper, IHttpContextAccessor httpContextAccessor, IAuthService authService, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _consultantRepo = _unitOfWork.GetRepository<Consultant>();
            _businessRepo = _unitOfWork.GetRepository<Business>();
            _categoryRepo = _unitOfWork.GetRepository<Category>();

        }

        public async Task<string> AddBusiness(CreateBusinessRequest businessRequest)
        {
            if (businessRequest.File == null || businessRequest.File.Length == 0)
            {
                throw new Exception("Image file is required.");
            }

            // Validate file size (max 5MB)
            if (businessRequest.File.Length > 5 * 1024 * 1024) // 5 MB
            {
                throw new Exception("File size exceeds the 5MB limit.");
            }

            // Validate file type (only jpg, jpeg, png)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(businessRequest.File.FileName).ToLower();
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

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(businessRequest.File.FileName);
            var fullPath = Path.Combine(pathToSave, fileName);
            var dbPath = Path.Combine(folderName, fileName).Replace('\\', '/');

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await businessRequest.File.CopyToAsync(stream);
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                throw new Exception("User not found.");
            }

            var business = _mapper.Map<Business>(businessRequest);
            business.ImagePath = dbPath;  // Set the image path

            // Fetch consultant
            Consultant consultant = await _consultantRepo.GetSingleByAsync(s => s.UserId == userId);
            if (consultant == null)
            {
                throw new Exception("Consultant not found.");
            }

            if (!consultant.IsVerified)
            {
                throw new Exception("Your account has not been verified. Please verify your account before creating a business.");
            }

            business.ConsultantId = consultant.Id;

            // Check if the category exists
            var category = await _categoryRepo.GetByIdAsync(businessRequest.CategoryId);
            if (category == null)
            {
                throw new Exception("Invalid Category ID.");
            }

            business.Category = category;

            // Set DateCreated to current UTC time
            business.DateCreated = DateTime.UtcNow;

            await _businessRepo.AddAsync(business);
            await _unitOfWork.SaveChangesAsync();

            var result = new { success = true, message = "Business created successfully." };
            return JsonConvert.SerializeObject(result);
        }


        public async Task<string> AddCategory(CreateCategoryRequest categoryRequest)
        {
           /* var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                throw new Exception("User not found");
            }
*/
            var category = _mapper.Map<Category>(categoryRequest);

          /*  Consultant consultant  = await _consultantRepo.GetSingleByAsync(s => s.UserId == userId);*/
/*
            if (consultant == null)
            {
                throw new Exception("Consultant not found");
            }

            category.ConsultantId = consultant.Id;*/

            await _categoryRepo.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var result = new { success = true, message = "category created successfully" };
            return JsonConvert.SerializeObject(result);

        }

    }
}
