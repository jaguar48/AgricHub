using AgricHub.BLL.Interfaces.IUserServices;
using AgricHub.Shared.DTO_s.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.Presentation.Controllers.UserController
{



    [ApiController]
    [Route("api/profile/consultant")]
    [Authorize(Roles = "Consultant")]
    public class ConsultantProfileController : ControllerBase
    {
        private readonly IConsultantProfileService _profileService;
        private readonly ILogger<ConsultantProfileController> _logger;

        public ConsultantProfileController(
            IConsultantProfileService profileService,
            ILogger<ConsultantProfileController> logger)
        {
            _profileService = profileService;
            _logger = logger;
        }

        /// <summary>
        /// Get current consultant profile
        /// </summary>
        [HttpGet]
        [SwaggerOperation("Get consultant profile")]
        [SwaggerResponse(200, "Profile retrieved successfully")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var profile = await _profileService.GetMyProfileAsync();
                return Ok(new { success = true, data = profile });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update consultant profile
        /// </summary>
        [HttpPut]
        [SwaggerOperation("Update consultant profile")]
        [SwaggerResponse(200, "Profile updated successfully")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateConsultantProfileRequest request)
        {
            try
            {
                await _profileService.UpdateProfileAsync(request);
                return Ok(new { success = true, message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get list of Nigerian banks
        /// </summary>
        [HttpGet("banks")]
        [SwaggerOperation("Get list of banks")]
        [SwaggerResponse(200, "Banks retrieved successfully")]
        public async Task<IActionResult> GetBanks()
        {
            try
            {
                var banks = await _profileService.GetBanksAsync();
                return Ok(new { success = true, data = banks });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving banks");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Verify bank account details
        /// </summary>
        [HttpPost("verify-account")]
        [SwaggerOperation("Verify bank account")]
        [SwaggerResponse(200, "Account verified successfully")]
        public async Task<IActionResult> VerifyBankAccount([FromBody] VerifyAccountRequest request)
        {
            try
            {
                var details = await _profileService.VerifyBankAccountAsync(
                    request.AccountNumber,
                    request.BankCode);

                return Ok(new { success = true, data = details });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying account");
                return BadRequest(new { success = false, message = "Invalid account details" });
            }
        }

        /// <summary>
        /// Update bank details for payouts
        /// </summary>
        [HttpPut("bank-details")]
        [SwaggerOperation("Update bank details")]
        [SwaggerResponse(200, "Bank details updated successfully")]
        public async Task<IActionResult> UpdateBankDetails([FromBody] UpdateBankDetailsRequest request)
        {
            try
            {
                await _profileService.UpdateBankDetailsAsync(request);
                return Ok(new { success = true, message = "Bank details updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bank details");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Upload profile picture
        /// </summary>
        [HttpPost("avatar")]
        [SwaggerOperation("Upload profile picture")]
        [SwaggerResponse(200, "Avatar uploaded successfully")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "No file uploaded" });

                var avatarUrl = await _profileService.UploadAvatarAsync(file);
                return Ok(new { success = true, message = "Avatar uploaded successfully", data = new { avatarUrl } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Change password
        /// </summary>
        [HttpPost("change-password")]
        [SwaggerOperation("Change password")]
        [SwaggerResponse(200, "Password changed successfully")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                await _profileService.ChangePasswordAsync(request);
                return Ok(new { success = true, message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    // Request DTO
    public record VerifyAccountRequest(string AccountNumber, string BankCode);

}