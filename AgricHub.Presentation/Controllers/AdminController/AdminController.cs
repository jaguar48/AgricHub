using AgricHub.BLL.Interfaces.IAdminService;
using AgricHub.Shared.DTO_s;
using AgricHub.Shared.DTO_s.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.Presentation.Controllers.AdminController
{

    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController(IAdminService adminService) : ControllerBase
    {
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
            => Ok(await adminService.GetStatsAsync());

        [HttpGet("reviews")]
        public async Task<IActionResult> GetReviews([FromQuery] int? minRating = null)
            => Ok(await adminService.GetReviewsAsync(minRating));

        [HttpDelete("reviews/{id:int}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try { await adminService.DeleteReviewAsync(id); return NoContent(); }
            catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); }
        }

        [HttpGet("verifications")]
        public async Task<IActionResult> GetVerifications([FromQuery] bool? verified = null)
            => Ok(await adminService.GetVerificationsAsync(verified));

        [HttpPatch("verifications/{id:int}")]
        public async Task<IActionResult> UpdateVerification(int id, [FromBody] UpdateVerificationRequest req)
        {
            try { await adminService.UpdateVerificationAsync(id, req); return NoContent(); }
            catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? role = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
            => Ok(await adminService.GetUsersAsync(role, search, page, pageSize));

        [HttpGet("consultants")]
        public async Task<IActionResult> GetConsultants(
            [FromQuery] bool? verifiedOnly = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
            => Ok(await adminService.GetConsultantsAsync(verifiedOnly, search, page, pageSize));

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
            => Ok(await adminService.GetCategoriesAsync());

        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest req)
        {
            var cat = await adminService.CreateCategoryAsync(req);
            return CreatedAtAction(nameof(GetCategories), new { id = cat.Id }, cat);
        }

        [HttpDelete("categories/{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try { await adminService.DeleteCategoryAsync(id); return NoContent(); }
            catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); }
        }
    }
}