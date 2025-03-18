using AgricHub.DAL.Entities;
using Microsoft.AspNetCore.Authentication;

namespace AgricHub.BLL.Implementations.UserServices;


public record AuthResult(
    bool Succeeded = false,
    AuthenticationProperties? Properties = null,
    bool IsChallenge = false,
    ApplicationUser? User = null,
    string? Error = null,
    IEnumerable<string>? Errors = null,
    string? Provider = null
    )
{

    public static AuthResult Challenge(AuthenticationProperties properties, string provider) => new(IsChallenge: true, Properties: properties, Provider: provider);

    public static AuthResult Failure(string errorMessage) => new(false, Error: errorMessage);

    public static AuthResult Success(ApplicationUser user) => new(true, User: user);
}
