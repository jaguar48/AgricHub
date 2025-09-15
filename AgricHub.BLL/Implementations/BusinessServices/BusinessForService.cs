using AgricHub.BLL.Interfaces.IAgrichub_Services;
using AgricHub.BLL.Interfaces.IUserServices;
using AgricHub.Contracts;
using AgricHub.DAL.Entities;
using AgricHub.DAL.Entities.Models;
using AgricHub.Shared.DTO_s.Request;
using AgricHub.Shared.DTO_s.Response;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace AgricHub.BLL.Implementations.AgrichubServices
{
    public class BusinessForService : IBusinessForService
    {
        private readonly IRepository<Service> _servicesRepo;
        private readonly IRepository<Business> _businessRepo;
        private readonly IRepository<Consultant> _consultantRepo;
        private readonly IRepository<ServicePackage> _servicePackageRepo;
        private readonly IUnitOfWork _unitOfWork;
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
            _servicePackageRepo = _unitOfWork.GetRepository<ServicePackage>();
        }

        public async Task<string> AddServiceAsync(CreateServiceRequest serviceRequest)
        {

            if (!string.IsNullOrEmpty(serviceRequest.PackagesJson))
            {
                serviceRequest.Packages = JsonConvert.DeserializeObject<List<ServicePackageRequest>>(serviceRequest.PackagesJson);
            }



            // Log the incoming Packages count for debugging
            Console.WriteLine($"Received {serviceRequest.Packages?.Count ?? 0} packages in CreateServiceRequest");

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

            var fileName = Guid.NewGuid().ToString() + fileExtension;
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

            // Validate category
            var category = await _unitOfWork.GetRepository<Category>().GetByIdAsync(serviceRequest.CategoryId);
            if (category == null)
            {
                throw new Exception("Invalid Category ID.");
            }

            var service = _mapper.Map<Service>(serviceRequest);
            service.ImagePath = dbPath;
            service.BusinessId = business.Id;
            service.CategoryId = category.Id;
            service.DateCreated = DateTime.UtcNow;

            // Add packages
            if (serviceRequest.Packages?.Any() == true)
            {
                Console.WriteLine("Mapping provided packages");
                service.Packages = _mapper.Map<List<ServicePackage>>(serviceRequest.Packages);
                foreach (var package in service.Packages)
                {
                    package.Service = service;
                    package.CreatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                Console.WriteLine("Creating default package");
                service.Packages.Add(new ServicePackage
                {
                    PackageName = "Basic",
                    Price = serviceRequest.Price,
                    Description = "Default package",
                    IncludesOnsiteVisit = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _servicesRepo.AddAsync(service);
            await _unitOfWork.SaveChangesAsync();

            return JsonConvert.SerializeObject(new { success = true, message = "Service created successfully." });
        }

        public async Task<string> UpdateServiceAsync(int serviceId, CreateServiceRequest serviceRequest)
        {
            if (!string.IsNullOrEmpty(serviceRequest.PackagesJson))
            {
                serviceRequest.Packages = JsonConvert.DeserializeObject<List<ServicePackageRequest>>(serviceRequest.PackagesJson);
            }
            var service = await _servicesRepo.GetSingleByAsync(s => s.Id == serviceId,
                include: q => q.Include(s => s.Packages));
            if (service == null)
            {
                throw new Exception("Service not found.");
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                throw new Exception("User not found.");
            }

            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == userId);
            if (consultant == null)
            {
                throw new UnauthorizedAccessException("Consultant not found.");
            }

            var business = await _businessRepo.GetSingleByAsync(b => b.Id == service.BusinessId && b.ConsultantId == consultant.Id);
            if (business == null)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this service.");
            }

            // Validate category
            var category = await _unitOfWork.GetRepository<Category>().GetByIdAsync(serviceRequest.CategoryId);
            if (category == null)
            {
                throw new Exception("Invalid Category ID.");
            }

            // Update service fields
            service.ServiceName = serviceRequest.ServiceName;
            service.Description = serviceRequest.Description;
            service.Price = serviceRequest.Price;
            service.CategoryId = category.Id;

            if (serviceRequest.File != null && serviceRequest.File.Length > 0)
            {
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

                var fileName = Guid.NewGuid().ToString() + fileExtension;
                var fullPath = Path.Combine(pathToSave, fileName);
                var dbPath = Path.Combine(folderName, fileName).Replace('\\', '/');

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await serviceRequest.File.CopyToAsync(stream);
                }

                service.ImagePath = dbPath;
            }

            // Update packages
            var existingPackageIds = service.Packages.Select(p => p.Id).ToList();
            var updatedPackageIds = serviceRequest.Packages.Select(p => p.Id).Where(id => id > 0).ToList();

            // Remove deleted packages
            var packagesToRemove = service.Packages.Where(p => !updatedPackageIds.Contains(p.Id)).ToList();
            foreach (var package in packagesToRemove)
            {
                _servicePackageRepo.Delete(package);
            }

            // Update or add packages
            foreach (var packageRequest in serviceRequest.Packages)
            {
                var existingPackage = service.Packages.FirstOrDefault(p => p.Id == packageRequest.Id);
                if (existingPackage != null)
                {
                    _mapper.Map(packageRequest, existingPackage);
                    existingPackage.CreatedAt = DateTime.UtcNow;
                }
                else
                {
                    var newPackage = _mapper.Map<ServicePackage>(packageRequest);
                    newPackage.ServiceId = service.Id;
                    newPackage.CreatedAt = DateTime.UtcNow;
                    service.Packages.Add(newPackage);
                }
            }

            service.DateCreated = DateTime.UtcNow;

            _servicesRepo.Update(service);
            await _unitOfWork.SaveChangesAsync();

            return JsonConvert.SerializeObject(new { success = true, message = "Service updated successfully." });
        }

        public async Task<ViewServiceResponse> ViewServiceAsync(int serviceId)
        {
            var service = await _servicesRepo.GetSingleByAsync(s => s.Id == serviceId,
                include: q => q.Include(s => s.Business).Include(s => s.Category).Include(s => s.Packages));
            if (service == null)
            {
                throw new Exception("Service not found.");
            }

            return _mapper.Map<ViewServiceResponse>(service);
        }

        public async Task<IEnumerable<ViewServiceResponse>> ViewAllServicesAsync()
        {
            var services = await _servicesRepo.GetAllAsync(
                include: q => q.Include(s => s.Business).Include(s => s.Category).Include(s => s.Packages));
            return _mapper.Map<IEnumerable<ViewServiceResponse>>(services);
        }

        public async Task<IEnumerable<ViewServiceResponse>> ViewOwnBusinessServicesAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == userId);
            if (consultant == null)
            {
                throw new UnauthorizedAccessException("Consultant not found.");
            }

            var business = await _businessRepo.GetSingleByAsync(b => b.ConsultantId == consultant.Id);
            if (business == null)
            {
                throw new Exception("No business found for this user.");
            }

            var services = await _servicesRepo.GetAllAsync(s => s.BusinessId == business.Id,
                include: q => q.Include(s => s.Business).Include(s => s.Category).Include(s => s.Packages));
            return _mapper.Map<IEnumerable<ViewServiceResponse>>(services);
        }

        public async Task<string> DeleteServiceAsync(int serviceId)
        {
            var service = await _servicesRepo.GetSingleByAsync(s => s.Id == serviceId);
            if (service == null)
            {
                throw new Exception("Service not found.");
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                throw new Exception("User not found.");
            }

            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == userId);
            if (consultant == null)
            {
                throw new UnauthorizedAccessException("Consultant not found.");
            }

            var business = await _businessRepo.GetSingleByAsync(b => b.Id == service.BusinessId && b.ConsultantId == consultant.Id);
            if (business == null)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this service.");
            }

            _servicesRepo.Delete(service);
            await _unitOfWork.SaveChangesAsync();

            return JsonConvert.SerializeObject(new { success = true, message = "Service deleted successfully." });
        }
    }
}