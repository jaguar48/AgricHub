using AgricHub.BLL.Interfaces.ChatServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AgricHub.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ISendbirdService _sendbirdService;

        public ChatController(ISendbirdService sendbirdService)
        {
            _sendbirdService = sendbirdService;
        }

        [HttpPost("create-channel")]
        public async Task<IActionResult> CreateChannel([FromBody] ChannelRequest request)
        {
            try
            {
                // Create the Sendbird group channel
                var channelUrl = await _sendbirdService.CreateGroupChannelAsync(
                    request.AgropreneurId,
                    request.ConsultantId
                );

                return Ok(new { success = true, channelUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpPost("create-sendbird-user")]
        public async Task<IActionResult> CreateSendbirdUser()
        {
            try
            {
                var result = await _sendbirdService.CreateSendbirdUserAsync();
                return Ok(new { success = true, user = JsonConvert.DeserializeObject<object>(result) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        public class ChannelRequest
        {
            public string AgropreneurId { get; set; }
            public string ConsultantId { get; set; }
        }
    }
}
