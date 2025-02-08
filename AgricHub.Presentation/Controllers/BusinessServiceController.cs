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

        // Existing method to add a service
        [HttpPost("add")]
        [SwaggerOperation("Adds a service to an existing business.")]
        [SwaggerResponse(200, "The service has been successfully added.", typeof(CreateServiceRequest))]
        public async Task<IActionResult> AddServiceToBusiness([FromForm] CreateServiceRequest serviceRequest)
        {
            try
            {
                var result = await _businessForService.AddServiceAsync(serviceRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }

            
        }

        // New method to update an existing service
        [HttpPut("update/{serviceId}")]
        [SwaggerOperation("Updates an existing service.")]
        [SwaggerResponse(200, "The service has been successfully updated.", typeof(CreateServiceRequest))]
        [SwaggerResponse(400, "Bad request. Invalid input data.")]
        public async Task<IActionResult> UpdateService([FromRoute] int serviceId, [FromForm] CreateServiceRequest serviceRequest)
        {
            try
            {
                // Call the business logic to update the service
                var result = await _businessForService.UpdateServiceAsync(serviceId, serviceRequest);

                return Ok(JsonConvert.DeserializeObject(result)); // Return a success response
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message }); // Handle errors
            }
        }
    }
}
