using AgricHub.BLL.Interfaces.IUserServices;
using AgricHub.DAL.Entities;
using AgricHub.Shared.DTO_s.Request;
using AgricHub.Shared.DTO_s.Request.AuthService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AgricHub.BLL.Implementations.UserServices
{
    public sealed class UserService : IUserServices
    {

        private readonly ILogger<UserService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;



        public UserService(UserManager<ApplicationUser> userManager, ILogger<UserService> logger)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<ApplicationUser?> FindOrCreateUserAsync(ExternalAuthInfo authInfo)
        {
            var user = await _userManager.FindByLoginAsync(authInfo.Provider, authInfo.ProviderKey);
            if (user != null) return user;

            user = await _userManager.FindByEmailAsync(authInfo.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = authInfo.Email,
                    Email = authInfo.Email,
                    FirstName = authInfo.Name ?? "",
                    LastName = authInfo.Name ?? "",
                    Address = authInfo.Address,
                    NormalizedUserName = authInfo.Email,
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded) return null;
            }

            await _userManager.AddLoginAsync(user, new UserLoginInfo(
                authInfo.Provider,
                authInfo.ProviderKey,
                authInfo.Provider));

            return user;
        }

        public async Task<ApplicationUser> RegisterUser(UserForRegistrationRequest Request)
        {

            /*_logger.LogInfo("Checking if user exist, if not create the user.");*/
            var existingUser = await _userManager.FindByEmailAsync(Request.Email.Trim().ToLower());
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email exists!");
            }

            var user = new ApplicationUser
            {
                FirstName = Request.FirstName,
                LastName = Request.LastName,
                UserName = Request.UserName,
                Email = Request.Email,
                PhoneNumber = Request.PhoneNumber,
                CountryId = Request.CountryId,
                StateId = Request.StateId,
                Address = Request.Address

            };



            var result = await _userManager.CreateAsync(user, Request.Password);
            if (!result.Succeeded)
            {

                string errMsg = string.Join("\n", result.Errors.Select(x => x.Description));

                throw new InvalidOperationException($"Failed to create user:\n{errMsg}");
            }

            return user;

        }


    }
}
