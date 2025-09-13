using AgricHub.BLL.Interfaces;
using AgricHub.BLL.Interfaces.IAgrichub_Services;
using AgricHub.BLL.Interfaces.IChatServices;
using AgricHub.Shared.DTO_s.Request;
using AgricHub.Shared.DTO_s.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        
        public async Task<IActionResult> InitiateChat([FromBody] InitiateChatRequest request)
        {
           
                var channelUrl = await _chatService.InitiateChatAsync(request.ConsultantUserId, request.ServiceId);
                return Ok(new { success = true, channelUrl });
           
        }

        //[HttpPost("create-channel")]
        //[Authorize(Roles = "Customer")]
        //public async Task<IActionResult> CreateChannel([FromBody] InitiateChatRequest request)
        //{
        //    try
        //    {
        //        var channelUrl = await _chatService.CreateChannelAsync(request.ConsultantUserId);
        //        return Ok(new { success = true, channelUrl });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpPost("create-sendbird-user")]
        //[Authorize(Roles = "Customer,Consultant")]
        //public async Task<IActionResult> CreateSendbirdUser()
        //{
        //    try
        //    {
        //        var result = await _chatService.CreateSendbirdUserAsync();
        //        return Ok(new { success = true, user = JsonConvert.DeserializeObject<object>(result) });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { success = false, message = ex.Message });
        //    }
        //}

        [HttpGet("my-chats")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyChats()
        {
           
                var chats = await _chatService.GetMyChatsAsync();
                return Ok(new { success = true, chats });
           
          
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