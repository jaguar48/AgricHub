using AgricHub.BLL.Interfaces;
using AgricHub.BLL.Interfaces.IChatServices;
using AgricHub.Shared.DTO_s.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgricHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("initiate")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> InitiateChat([FromBody] InitiateChatRequest request)
        {
            try
            {
                var channelUrl = await _chatService.InitiateChatAsync(request);
                return Ok(new { success = true, channelUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("custom-offer")]
        [Authorize(Roles = "Consultant")]
        public async Task<IActionResult> CreateCustomOffer([FromBody] CustomOfferRequest request)
        {
            try
            {
                var offer = await _chatService.CreateCustomOfferAsync(request);
                return Ok(new { success = true, offer });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("custom-offer/{offerId}/accept")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AcceptCustomOffer(Guid offerId)
        {
            try
            {
                var offer = await _chatService.AcceptCustomOfferAsync(offerId);
                return Ok(new { success = true, offer });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("custom-offer/{offerId}/reject")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RejectCustomOffer(Guid offerId, [FromBody] string reason)
        {
            try
            {
                var offer = await _chatService.RejectCustomOfferAsync(offerId, reason);
                return Ok(new { success = true, offer });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("my-chats")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyChats()
        {
            try
            {
                var chats = await _chatService.GetMyChatsAsync();
                return Ok(new { success = true, chats });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("consultant-chats")]
        [Authorize(Roles = "Consultant")]
        public async Task<IActionResult> GetConsultantChats()
        {
            try
            {
                var chats = await _chatService.GetConsultantChatsAsync();
                return Ok(new { success = true, chats });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}