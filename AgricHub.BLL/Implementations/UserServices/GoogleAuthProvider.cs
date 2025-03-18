using System;
using System.Security.Claims;
using AgricHub.BLL.Interfaces.AuthService;
using AgricHub.BLL.Interfaces.IUserServices;
using AgricHub.DAL.Entities;
using AgricHub.Shared.DTO_s.Request.AuthService;
using AgricHub.Shared.DTO_s.Response.AuthService;
using Microsoft.AspNetCore.Identity;


namespace AgricHub.BLL.Implementations.UserServices;

// Services/Auth/GoogleAuthProvider.cs
public class GoogleAuthProvider(
    SignInManager<ApplicationUser> signInManager,
    IUserServices userService) : IExternalAuthProvider
{
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IUserServices _userService = userService;

    public bool IsSupportedProvider(string provider) => provider == "Google";

    public async Task<ExternalAuthInfo?> GetExternalAuthInfoAsync()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null) return null;

        return new ExternalAuthInfo(
            info.LoginProvider,
            info.ProviderKey,
            info.Principal.FindFirstValue(ClaimTypes.Email)!,
            info.Principal.FindFirstValue(ClaimTypes.Name),
            info.Principal.FindFirstValue(ClaimTypes.StreetAddress),
            // info.Principal.FindFirstValue("urn:google:image"),
            info.Principal.Claims
        );
    }

    public async Task<AuthResult> ProcessExternalAuthAsync(ExternalAuthInfo authInfo)
    {
        var user = await _userService.FindOrCreateUserAsync(authInfo);
        if (user == null)
            return AuthResult.Failure("User creation failed");

        await _signInManager.SignInAsync(user, isPersistent: false);
        return AuthResult.Success(user);
    }
}
