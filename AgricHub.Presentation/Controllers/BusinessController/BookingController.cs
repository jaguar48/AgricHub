using AgricHub.BLL.Interfaces.IBusinessServices;
using AgricHub.Shared.DTO_s.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AgricHub.Presentation.Controllers.BusinessController
{
    [ApiController]
    [Route("api/agrichub/booking")]
    [Authorize] // Ensure only logged-in users hit these endpoints
    public class BookingController : ControllerBase
    {
        private readonly IConsultationService _consultationService;

        public BookingController(IConsultationService consultationService)
        {
            _consultationService = consultationService;
        }

        // ============= CONSULTATION LIFECYCLE =============

        [HttpPost("book")]
        [Authorize(Roles = "Customer")]
        [SwaggerOperation("Customer books a consultation. Payment deducted from wallet and held in escrow.")]
        public async Task<IActionResult> BookConsultation([FromBody] ConsultationBookingRequest dto)
        {
            try
            {
                var result = await _consultationService.BookConsultationAsync(dto);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Consultant")]
        [SwaggerOperation("Consultant approves a pending consultation.")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveRequest? request)
        {
            try
            {
                var result = await _consultationService.ApproveConsultationAsync(id, request?.Notes);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Consultant")]
        [SwaggerOperation("Consultant rejects a pending consultation. Customer receives full refund.")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest request)
        {
            try
            {
                var result = await _consultationService.RejectConsultationAsync(id, request.Reason);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id}/start")]
        [Authorize(Roles = "Consultant")]
        [SwaggerOperation("Consultant starts an approved consultation.")]
        public async Task<IActionResult> Start(Guid id)
        {
            try
            {
                var result = await _consultationService.StartConsultationAsync(id);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Consultant")]
        [SwaggerOperation("Consultant completes a consultation. Escrow funds released to consultant wallet.")]
        public async Task<IActionResult> Complete(Guid id)
        {
            try
            {
                var result = await _consultationService.CompleteConsultationAsync(id);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        [SwaggerOperation("Customer or Consultant cancels a consultation. Full refund to customer.")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelRequest? request)
        {
            try
            {
                var result = await _consultationService.CancelConsultationAsync(id, request?.Reason);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============= NO-SHOW REPORTING (NEW) =============

        [HttpPost("{id}/report-consultant-noshow")]
        [Authorize(Roles = "Customer")]
        [SwaggerOperation("Customer reports consultant no-show. Full refund to customer. Consultant's no-show count increases.")]
        public async Task<IActionResult> ReportConsultantNoShow(Guid id)
        {
            try
            {
                var result = await _consultationService.ReportConsultantNoShowAsync(id);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id}/report-customer-noshow")]
        [Authorize(Roles = "Consultant")]
        [SwaggerOperation("Consultant reports customer no-show. 50% to consultant, 50% refunded to customer. Customer's no-show count increases.")]
        public async Task<IActionResult> ReportCustomerNoShow(Guid id)
        {
            try
            {
                var result = await _consultationService.ReportCustomerNoShowAsync(id);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============= VIEW CONSULTATIONS (NEW) =============

        [HttpGet("my-consultations")]
        [Authorize(Roles = "Customer")]
        [SwaggerOperation("Customer retrieves all their consultations with escrow status.")]
        public async Task<IActionResult> GetMyConsultations()
        {
            try
            {
                var result = await _consultationService.GetMyConsultationsAsync();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("consultant-consultations")]
        [Authorize(Roles = "Consultant")]
        [SwaggerOperation("Consultant retrieves all their consultations with pending payouts.")]
        public async Task<IActionResult> GetConsultantConsultations()
        {
            try
            {
                var result = await _consultationService.GetConsultantConsultationsAsync();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    // ============= REQUEST MODELS =============

    public class ApproveRequest
    {
        public string? Notes { get; set; }
    }

    public class RejectRequest
    {
        public string Reason { get; set; }
    }

    public class CancelRequest
    {
        public string? Reason { get; set; }
    }
}