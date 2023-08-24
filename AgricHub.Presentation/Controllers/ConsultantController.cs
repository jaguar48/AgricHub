using AgricHub.BLL.Interfaces.IUserServices;
using AgricHub.Presentation.Filters;
using AgricHub.Shared.DTO_s.Request;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AgricHub.Presentation.Controllers
{
    [ApiController]
    [Route("api/agrichub")]
    public class ConsultantController : ControllerBase
    {
        private readonly IConsultantService _consultantServices;
        /* private readonly ISellerProfileServices _sellerProfileServices;*/

        public ConsultantController(IConsultantService consultantServices)
        {
            _consultantServices = consultantServices;
            /*_sellerProfileServices = sellerProfileServices;*/
        }

        [HttpPost("register")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [SwaggerOperation("Registers a new consultant.")]
        [SwaggerResponse(200, "The consultant has been successfully registered.", typeof(ConsultantRegistrationRequest))]
        public async Task<IActionResult> RegisterConsultant([FromBody] ConsultantRegistrationRequest consultantRegistrationRequest)
        {

            var result = await _consultantServices.RegisterConsultant(consultantRegistrationRequest);
            return Ok(result);
        }
    }
}
