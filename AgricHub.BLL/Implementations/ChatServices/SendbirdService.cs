
using AgricHub.BLL.Interfaces.ChatServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

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
        {
            throw new Exception("User context is missing.");
        }

        var requestBody = new
        {
            user_id = userId,
            nickname = username,
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
            throw new Exception($"Failed to create Sendbird user: {responseContent}");
        }

        return responseContent;
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

        if (!response.IsSuccessStatusCode && !responseContent.Contains("user_id already exists"))
        {
            throw new Exception($"Failed to create Sendbird user: {responseContent}");
        }

        return responseContent;
    }





    public async Task<string> CreateGroupChannelAsync(string agropreneurUserId, string consultantUserId)
    {
        var url = $"https://api-{_sendbirdAppId}.sendbird.com/v3/group_channels";

        var payload = new
        {
            name = $"Consultation_{agropreneurUserId}_{consultantUserId}",
            user_ids = new[] { agropreneurUserId, consultantUserId },
            is_distinct = true
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
        };

       
        request.Headers.Add("Api-Token", _sendbirdApiToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Sendbird channel creation failed: {errorContent}");
        }

        var result = await response.Content.ReadAsStringAsync();
        dynamic json = JsonConvert.DeserializeObject(result);
        return json.channel_url;
    }



}
