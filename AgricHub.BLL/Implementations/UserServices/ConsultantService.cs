using AgricHub.BLL.Helpers;
using AgricHub.BLL.Interfaces.IUserServices;
using AgricHub.Contracts;
using AgricHub.DAL.Entities;
using AgricHub.DAL.Entities.Models;
using AgricHub.Shared.DTO_s.Request;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AgricHub.BLL.Implementations.UserServices.UserServices
{
    public sealed class ConsultantService : IConsultantService
    {
        private readonly IRepository<Consultant> _consultantRepo;
        private readonly IRepository<Wallet> _walletRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserServices _userServices;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthService _authService;

        public ConsultantService(
            IAuthService authService,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IUserServices userServices)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _authService = authService;
            _userServices = userServices;
            _consultantRepo = _unitOfWork.GetRepository<Consultant>();
            _walletRepo = _unitOfWork.GetRepository<Wallet>();
        }

        public async Task<string> RegisterConsultant(ConsultantRegistrationRequest request)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1️⃣ Create the Application User
                var user = await _userServices.RegisterUser(new UserForRegistrationRequest
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Password = request.Password,
                    UserName = request.UserName,
                    CountryId = request.CountryId,
                    StateId = request.StateId,
                    Address = request.Address
                });

                await _userManager.AddToRoleAsync(user, "Consultant");

                // 2️⃣ Create Consultant profile
                var consultant = new Consultant
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    BusinessName = request.BusinessName,
                    CountryId = request.CountryId,
                    StateId = request.StateId,
                    Address = request.Address,
                    UserId = user.Id
                };

                await _consultantRepo.AddAsync(consultant);

                // 3️⃣ Create Wallet for Consultant
                await CreateConsultantWalletAsync(consultant);

                // 4️⃣ Save all changes together
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // 5️⃣ Optional: send email verification
                // var verificationToken = Guid.NewGuid().ToString();
                // var emailSent = await _authService.SendVerificationEmail(request.Email, verificationToken);
                // if (emailSent)
                // {
                //     user.VerificationToken = verificationToken;
                //     await _userManager.UpdateAsync(user);
                // }

                var result = new
                {
                    success = true,
                    message = "Registration successful! Please check your email for the verification link."
                };
                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                // Rollback all changes on failure
                await _unitOfWork.RollbackTransactionAsync();

                // Clean up user if created
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                    await _userManager.DeleteAsync(existingUser);

                var result = new
                {
                    success = false,
                    message = $"Registration failed: {ex.Message}"
                };
                return JsonConvert.SerializeObject(result);
            }
        }

        private async Task CreateConsultantWalletAsync(Consultant consultant)
        {
            Wallet wallet = new()
            {
                WalletNo = WalletIdGenerator.GenerateWalletId(),
                Balance = 0,
                IsActive = true,
                ConsultantId = consultant.Id,
            };
            await _walletRepo.AddAsync(wallet);
        }
    }
}
