using AgricHub.Shared.DTO_s.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.BLL.Interfaces.IChatServices
{
    
        public interface IChatService
        {
            Task<string> InitiateChatAsync(string consultantUserId, int? serviceId = null);
            Task<IEnumerable<ChatSessionResponse>> GetMyChatsAsync();
            Task<IEnumerable<ChatSessionResponse>> GetConsultantChatsAsync();
        }
   
}
