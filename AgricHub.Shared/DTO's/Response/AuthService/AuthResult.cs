using AgricHub.DAL.Entities;

namespace AgricHub.Shared.DTO_s.Response.AuthService;


public record AuthResult(
    bool Succeeded,
    ApplicationUser User = null,
    string Error = null,
    IEnumerable<string> Errors = null);
