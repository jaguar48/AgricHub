using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.BLL.Interfaces.ChatServices
{
    public interface ISendbirdService
    {
        Task<string> CreateSendbirdUserAsync();
        Task<string> CreateGroupChannelAsync(string agropreneurUserId, string consultantUserId);
        Task<string> CreateSendbirdUserAsync(string userId, string nickname);
    }
}
