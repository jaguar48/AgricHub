using AgricHub.DAL.Entities;
using AgricHub.Shared.DTO_s.Request;
using AgricHub.Shared.DTO_s.Response;

namespace AgricHub.BLL.Interfaces.IBusinessServices
{
    public interface IConsultationService
    {
        Task<ConsultationResponse> BookConsultationAsync(ConsultationBookingRequest dto);

        Task<ConsultationResponse> ApproveConsultationAsync(Guid consultationId, string? notes = null);
        Task<ConsultationResponse> RejectConsultationAsync(Guid consultationId, string reason);
        Task<ConsultationResponse> StartConsultationAsync(Guid consultationId);
        Task<ConsultationResponse> CompleteConsultationAsync(Guid consultationId);
        Task<ConsultationResponse> CancelConsultationAsync(Guid consultationId, string? reason = null);

        // Queries
        Task<IEnumerable<ConsultationResponse>> GetMyConsultationsAsync();
        Task<IEnumerable<ConsultationResponse>> GetConsultantConsultationsAsync();
    }
}
