using AgricHub.BLL.Interfaces;
using AgricHub.DAL.Entities;
using AgricHub.Shared.DTO_s.Request;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.BLL.Implementations
{
    public sealed class UserServices : IUserServices
    {

        /*private readonly ILoggerManager _logger;*/
        private readonly UserManager<ApplicationUser> _userManager;



        public UserServices(UserManager<ApplicationUser> userManager)
        {
            /*_logger = logger;*/
            _userManager = userManager;
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
                PhoneNumber = Request.PhoneNumber
               
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
