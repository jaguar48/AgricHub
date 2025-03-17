namespace AgricHub.Shared.DTO_s.Request.AuthService;

public record LoginRequest(
    string ReturnUrl,
    string Provider);
