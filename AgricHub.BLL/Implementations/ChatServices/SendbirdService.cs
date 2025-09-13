using AgricHub.BLL.Interfaces.ChatServices;
using AgricHub.DAL.Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.BLL.Implementations.ChatServices
{
  

    public class SendbirdChannel
    {
        public string channel_url { get; set; }
        public List<SendbirdMember> members { get; set; }
        public bool is_distinct { get; set; }
    }

    public class SendbirdChannelResponse
    {
        public List<SendbirdChannel> channels { get; set; }
    }

    public class SendbirdService : ISendbirdService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly string _sendbirdAppId;
        private readonly string _sendbirdApiToken;

        public SendbirdService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = new HttpClient();
            _sendbirdAppId = configuration["Sendbird:AppId"];
            _sendbirdApiToken = configuration["Sendbird:ApiToken"];
        }

        public async Task<string> CreateSendbirdUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
                throw new Exception("User context is missing.");
            return await CreateSendbirdUserAsync(userId, username);
        }

        public async Task<string> CreateSendbirdUserAsync(string userId, string nickname)
        {
            var requestBody = new
            {
                user_id = userId,
                nickname = nickname,
                profile_url = "https://placehold.co/100x100.png"
            };

            var requestJson = JsonConvert.SerializeObject(requestBody);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://api-{_sendbirdAppId}.sendbird.com/v3/users")
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Add("Api-Token", _sendbirdApiToken);

            var response = await _httpClient.SendAsync(requestMessage);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (responseContent.Contains("user_id already exists"))
                {
                    var getRequest = new HttpRequestMessage(HttpMethod.Get, $"https://api-{_sendbirdAppId}.sendbird.com/v3/users/{userId}");
                    getRequest.Headers.Add("Api-Token", _sendbirdApiToken);
                    var getResponse = await _httpClient.SendAsync(getRequest);
                    return await getResponse.Content.ReadAsStringAsync();
                }
                throw new Exception($"Failed to create Sendbird user: {responseContent}");
            }

            return responseContent;
        }

        public async Task SendMessageAsync(string channelUrl, string senderUserId, string message, bool isSystemMessage = false, object? data = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));

            var url = $"https://api-{_sendbirdAppId}.sendbird.com/v3/group_channels/{channelUrl}/messages";
            var payload = new
            {
                message_type = "MESG",
                user_id = senderUserId,
                message,
                custom_type = isSystemMessage ? "system" : "user",
                data = data != null ? JsonConvert.SerializeObject(data) : null
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Api-Token", _sendbirdApiToken);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to send message: {content}");
        }

        public async Task<string> CreateGroupChannelAsync(string agropreneurUserId, string consultantUserId)
        {
            var existingChannel = await GetExistingChannelAsync(agropreneurUserId, consultantUserId);
            if (!string.IsNullOrEmpty(existingChannel))
                return existingChannel;

            var url = $"https://api-{_sendbirdAppId}.sendbird.com/v3/group_channels";
            var payload = new
            {
                name = $"Chat_{agropreneurUserId}_{consultantUserId}",
                user_ids = new[] { agropreneurUserId, consultantUserId },
                is_distinct = true
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Api-Token", _sendbirdApiToken);

            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Sendbird channel creation failed: {result}");

            var json = JsonConvert.DeserializeObject<SendbirdChannel>(result);
            return json.channel_url;
        }

        public async Task<string> GetExistingChannelAsync(string userId1, string userId2)
        {
            var url = $"https://api-{_sendbirdAppId}.sendbird.com/v3/group_channels?user_id={userId1}&show_member=true";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Api-Token", _sendbirdApiToken);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to fetch channels: {content}");

            var channelResponse = JsonConvert.DeserializeObject<SendbirdChannelResponse>(content);
            foreach (var channel in channelResponse.channels)
            {
                var memberIds = channel.members.Select(m => m.user_id).ToList();
                if (memberIds.Contains(userId1) && memberIds.Contains(userId2) && channel.is_distinct)
                    return channel.channel_url;
            }

            return null;
        }

        public async Task<string> EnsureSendbirdUserAsync(string userId, string nickname)
        {
            var getRequest = new HttpRequestMessage(HttpMethod.Get, $"https://api-{_sendbirdAppId}.sendbird.com/v3/users/{userId}");
            getRequest.Headers.Add("Api-Token", _sendbirdApiToken);

            var getResponse = await _httpClient.SendAsync(getRequest);
            if (getResponse.IsSuccessStatusCode)
            {
                return await getResponse.Content.ReadAsStringAsync(); 
            }

           
            return await CreateSendbirdUserAsync(userId, nickname);
        }

        public async Task SendAdminMessageAsync(string channelUrl, string message, object? data = null)
        {
            var url = $"https://api-{_sendbirdAppId}.sendbird.com/v3/group_channels/{channelUrl}/messages";
            var payload = new
            {
                message_type = "ADMM",  
                message,
                data = data != null ? JsonConvert.SerializeObject(data) : null
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Api-Token", _sendbirdApiToken);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to send admin message: {content}");
        }


    }
}