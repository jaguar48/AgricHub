using AgricHub.BLL.Interfaces.IUserServices;
using AgricHub.DAL.Entities;
using AgricHub.Presentation.Filters;
using AgricHub.Shared.DTO_s.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.Presentation.Controllers
{
    [ApiController]
    [Route("/api/agrichub/authentication")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authentication;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(IAuthService authentication, UserManager<ApplicationUser> userManager)
        {
            _authentication = authentication;
            _userManager = userManager;
        }


        [HttpPost("login")]

        [SwaggerOperation(Summary = "Authenticate user and create token", Description = "Authenticate user and create token.")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Token created successfully.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid user credentials.")]
        public async Task<IActionResult> Authenticate([FromBody] UserAuthenticationResponse user)
        {
            var response = await _authentication.ValidateUser(user);


            if (!response.Success)
                return BadRequest(response);

            return Ok(new { Token = await _authentication.CreateToken(), Role = response.Role });

        }


        [HttpGet("google-login")]

        public IActionResult Login(string? returnUrl = null)
        {
            // For APIs, return a URL to initiate external login
            var loginUrl = Url.Action("ExternalLogin", "Auth", new { provider = "Google", returnUrl });
            return Ok(new { LoginUrl = loginUrl });
        }


        [HttpGet("externallogin")]
        public async Task<IActionResult> ExternalLogin(string provider, string returnUrl)
        {
            // Generate the redirect URL for the callback
            var redirectUrl = Url.Action(
                "ExternalLoginCallback",  // Callback action name
                "Auth",           // Callback controller name
                new { ReturnUrl = returnUrl },  // Pass the return URL
                protocol: HttpContext.Request.Scheme  // Use the current request scheme (HTTP/HTTPS)
            );

            var result = await _authentication.ExternalLoginAsync(provider, returnUrl);

            if (result.IsChallenge)
            {
                // Redirect to the external provider's login page
                return Challenge(result.Properties, result.Provider!);
            }

            return BadRequest(new { Error = "Failed to initiate external login" });
        }


        [HttpGet("externallogincallback")]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var result = await _authentication.HandleExternalLoginCallbackAsync();

            if (result.Succeeded)
            {
                var token = await _authentication.CreateToken();
                return Ok(new { Token = token, User = result.User });
            }

            return Unauthorized(new { Error = "External login failed", Details = result.Errors });
        }

        // POST: api/Account/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _authentication.LogoutAsync();
            return Ok(new { Message = "Logged out successfully" });
        }

    }
}
