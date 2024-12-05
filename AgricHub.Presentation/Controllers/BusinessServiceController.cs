using AgricHub.BLL.Interfaces.IAgrichub_Services;
using AgricHub.Shared.DTO_s.Request;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

namespace AgricHub.Presentation.Controllers
{
    [ApiController]
    [Route("api/agrichub/service")]
    public class ServiceController : ControllerBase
    {
        private readonly IBusinessForService _businessForService;

        public ServiceController(IBusinessForService businessForService)
        {
            _businessForService = businessForService;
        }

      
        [HttpPost("add")]
        [SwaggerOperation("Adds a service to an existing business.")]
        [SwaggerResponse(200, "The service has been successfully added.", typeof(CreateServiceRequest))]
        public async Task<IActionResult> AddServiceToBusiness([FromForm] CreateServiceRequest serviceRequest)
        {
            try
            {
                
                var result = await _businessForService.AddService(serviceRequest);

               
                return Ok(JsonConvert.DeserializeObject(result));
            }
            catch (Exception ex)
            {
               
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

      
    }
}
