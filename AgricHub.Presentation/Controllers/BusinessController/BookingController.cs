using AgricHub.BLL.Interfaces.IBusinessServices;
using AgricHub.Shared.DTO_s.Request;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgricHub.Presentation.Controllers.BusinessController
{
    [ApiController]
    [Route("/api/agrichub/booking")]
    [Authorize] // ensure only logged-in users hit these endpoints
    public class BookingController : ControllerBase
    {
        private readonly IConsultationService _consultationService;

        public BookingController(IConsultationService consultationService)
        {
            _consultationService = consultationService;
        }

        // ✅ Customer: Book
        [HttpPost("book")]
        public async Task<IActionResult> BookConsultation([FromBody] ConsultationBookingRequest dto)
        {
           

            try
            {
                var result = await _consultationService.BookConsultationAsync(dto);
                
                return Ok(new { success = true, result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }


        }

        // ✅ Consultant: Approve
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] string? notes)
        {
            var result = await _consultationService.ApproveConsultationAsync(id, notes);
            return Ok(result);
        }

        // ✅ Consultant: Reject
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] string reason)
        {
            var result = await _consultationService.RejectConsultationAsync(id, reason);
            return Ok(result);
        }

        // ✅ Consultant: Start
        [HttpPost("{id}/start")]
        public async Task<IActionResult> Start(Guid id)
        {
            var result = await _consultationService.StartConsultationAsync(id);
            return Ok(result);
        }

        // ✅ Consultant: Complete
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(Guid id)
        {
            var result = await _consultationService.CompleteConsultationAsync(id);
            return Ok(result);
        }

        // ✅ Customer OR Consultant: Cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] string? reason)
        {
            var result = await _consultationService.CancelConsultationAsync(id, reason);
            return Ok(result);
        }
    }
}
