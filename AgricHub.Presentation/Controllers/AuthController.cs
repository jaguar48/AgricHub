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
    [Route("/agrichub/authentication")]
    public class AuthController:ControllerBase
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
    }
}
