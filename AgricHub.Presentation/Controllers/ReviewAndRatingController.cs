using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AgricHub.Presentation.Controllers
{
    [Route("api/agrichub/[controller]")]
    [ApiController]
    public class ReviewAndRatingController : ControllerBase
    {
         [HttpPost("rate")]
        
        [SwaggerOperation(Summary = "Rate Products", Description = "Authenticate user and create token.")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Token created successfully.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid user credentials.")]
        public async Task<IActionResult> Rate([FromBody] )
        {
            // var response = await _authentication.ValidateUser(user);


            // if (!response.Success)
            //     return BadRequest(response);

            // return Ok(new { Token = await _authentication.CreateToken(), Role = response.Role });

        }
    }
    }
}
