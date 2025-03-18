using AgricHub.DAL.Entities;
using Microsoft.AspNetCore.Authentication;

namespace AgricHub.Shared.DTO_s.Response.AuthService;


public record AuthResult(
    bool Succeeded,
    ApplicationUser? User = null,
    string? Error = null,
    IEnumerable<string>? Errors = null,
    AuthenticationProperties Properties,
    string? Provider = null,
    bool IsChallenge = false,
    )
{

    public static AuthResult Challenge(AuthenticationProperties properties, string provider) => new(IsChallenge: true, Properties: properties, Provider: provider  )

    public static AuthResult Failure(string errorMessage) => new(false, Error: errorMessage);

    public static AuthResult Success(ApplicationUser user) => new(true, User: user);
}
