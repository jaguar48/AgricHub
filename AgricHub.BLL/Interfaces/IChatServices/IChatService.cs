using AgricHub.Shared.DTO_s.Request;
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
        Task<string> InitiateChatAsync(InitiateChatRequest request);
        Task<IEnumerable<ChatSessionResponse>> GetMyChatsAsync();
        Task<IEnumerable<ChatSessionResponse>> GetConsultantChatsAsync();
        Task<CustomOfferResponse> CreateCustomOfferAsync(CustomOfferRequest request);
        Task<CustomOfferResponse> AcceptCustomOfferAsync(Guid offerId);
        Task<CustomOfferResponse> RejectCustomOfferAsync(Guid offerId, string reason);
    }
   
}
