using System.Security.Claims;

namespace AgricHub.Shared.DTO_s.Request.AuthService;

public record ExternalAuthInfo(
    string Provider,
    string ProviderKey,
    string Email,
    string Name,
    string ProfileImageUrl,
    IEnumerable<Claim> Claims);
