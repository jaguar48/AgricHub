using System;
using AgricHub.Shared.DTO_s.Request.AuthService;
using AgricHub.Shared.DTO_s.Response.AuthService;

namespace AgricHub.BLL.Interfaces.AuthService;


public interface IExternalAuthProvider
{
    bool IsSupportedProvider(string provider);
    Task<ExternalAuthInfo> GetExternalAuthInfoAsync();
    Task<AuthResult> ProcessExternalAuthAsync(ExternalAuthInfo authInfo);
}


