using System;
using AgricHub.Shared.DTO_s.Request.AuthService;

namespace AgricHub.BLL.Implementations.UserServices;


public interface IExternalAuthProvider
{
    bool IsSupportedProvider(string provider);
    Task<ExternalAuthInfo?> GetExternalAuthInfoAsync();
    Task<AuthResult> ProcessExternalAuthAsync(ExternalAuthInfo authInfo);
}