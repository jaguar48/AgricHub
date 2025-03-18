using AgricHub.DAL.Entities;
using Microsoft.AspNetCore.Authentication;

namespace AgricHub.Shared.DTO_s.Response.AuthService;


public record AuthResult(
    bool Succeeded,
    ApplicationUser? User = null,
    string? Error = null,
    IEnumerable<string>? Errors = null)
{
    public static AuthResult Challenge(AuthenticationProperties properties, string provider)
    {
        throw new NotImplementedException();
    }

    public static AuthResult Failure(string errorMessage) => new(false, Error: errorMessage);

    public static AuthResult Success(ApplicationUser user) => new(true, User: user);
}
