using AgricHub.Shared.DTO_s.Request;
using AgricHub.Shared.DTO_s.Response;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.BLL.Interfaces.IUserServices
{
    public interface IConsultantProfileService
    {
        Task<ConsultantProfileResponse> GetMyProfileAsync();
        Task UpdateProfileAsync(UpdateConsultantProfileRequest request);
        Task UpdateBankDetailsAsync(UpdateBankDetailsRequest request);
        Task ChangePasswordAsync(ChangePasswordRequest request);
        Task<string> UploadAvatarAsync(IFormFile file);
        Task<List<BankInfo>> GetBanksAsync();
        Task<BankAccountDetails> VerifyBankAccountAsync(string accountNumber, string bankCode);
    }
}
