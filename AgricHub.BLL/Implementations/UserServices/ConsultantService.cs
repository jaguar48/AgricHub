using AgricHub.BLL.Helpers;
using AgricHub.BLL.Interfaces.IUserServices;
using AgricHub.Contracts;
using AgricHub.DAL.Entities;
using AgricHub.DAL.Entities.Models;
using AgricHub.Shared.DTO_s.Request;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;


namespace AgricHub.BLL.Implementations.UserServices.UserServices
{

    public sealed class ConsultantService : IConsultantService
    {

        private readonly IRepository<Consultant> _consultantRepo;
        private readonly IUnitOfWork _unitOfWork;
        /*private readonly ILoggerManager _logger;*/
        private readonly IUserServices _userServices;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthService _authService;

        private readonly IRepository<Wallet> _walletRepo;


        public ConsultantService(IAuthService authService, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IUserServices userServices)
        {
            /*_logger = logger;*/
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _authService = authService;
            _userServices = userServices;
            _consultantRepo = _unitOfWork.GetRepository<Consultant>();
            _walletRepo = _unitOfWork.GetRepository<Wallet>();
        }


        public async Task<string> RegisterConsultant(ConsultantRegistrationRequest request)
        {
            
            /* _logger.LogInfo("Creating the Seller as a user first, before assigning the seller role to them and adding them to the Sellers table.");*/
            
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

            var consultant = new Consultant
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                BusinessName = request.BusinessName,
                CountryId = request.CountryId,
                StateId = request.StateId,
                Address = request.Address ,
                UserId = user.Id
            };

            await _consultantRepo.AddAsync(consultant);
            await CreateCustomerAccount(consultant);

           /* var verificationToken = Guid.NewGuid().ToString();
            var emailSent = await _authService.SendVerificationEmail(request.Email, verificationToken);*/

           /* if (emailSent)
            {

                user.VerificationToken = verificationToken;
                await _userManager.UpdateAsync(user);
*/

                var result = new { success = true, message = "Registration Successful! Please check your email for the verification link." };
                return JsonConvert.SerializeObject(result);
           /* }
            else
            {

                var result = new { success = false, message = "Failed to send verification email. Please try again later." };
                return JsonConvert.SerializeObject(result);
            }*/
        }



        private async Task CreateCustomerAccount(Consultant consultant)
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
