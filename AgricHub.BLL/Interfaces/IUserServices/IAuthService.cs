using AgricHub.DAL.Entities;
using AgricHub.Shared.DTO_s.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.BLL.Interfaces.IUserServices
{
    public interface IAuthService
    {
        Task<ServiceResponse<string>> ValidateUser(UserAuthenticationResponse response);
        
        Task<string> CreateToken();

        public Task<ApplicationUser> VerifyUser(string email, string verificationToken);
        public Task<bool> SendVerificationEmail(string email, string verificationToken);
        public Task<bool> ResetPassword(string email, string token, string newPassword);
        public Task<bool> SendPasswordResetEmail(string email, string resetToken);
    }
}
