using AgricHub.DAL.Entities;
using AgricHub.Shared.DTO_s.Request;
using AgricHub.Shared.DTO_s.Response;

namespace AgricHub.BLL.Interfaces.IBusinessServices
{
    public interface IConsultationService
    {
        // ============= CONSULTATION LIFECYCLE =============

        /// <summary>
        /// Customer books a consultation. Payment deducted from wallet and held in escrow.
        /// </summary>
        Task<ConsultationResponse> BookConsultationAsync(ConsultationBookingRequest dto);

        /// <summary>
        /// Consultant approves a pending consultation.
        /// </summary>
        Task<ConsultationResponse> ApproveConsultationAsync(Guid consultationId, string? notes = null);

        /// <summary>
        /// Consultant rejects a pending consultation. Full refund to customer.
        /// </summary>
        Task<ConsultationResponse> RejectConsultationAsync(Guid consultationId, string reason);

        /// <summary>
        /// Consultant starts an approved consultation.
        /// </summary>
        Task<ConsultationResponse> StartConsultationAsync(Guid consultationId);

        /// <summary>
        /// Consultant completes a consultation. Escrow funds released to consultant wallet.
        /// </summary>
        Task<ConsultationResponse> CompleteConsultationAsync(Guid consultationId);

        /// <summary>
        /// Customer or Consultant cancels a consultation. Full refund to customer.
        /// </summary>
        Task<ConsultationResponse> CancelConsultationAsync(Guid consultationId, string? reason = null);

        // ============= NO-SHOW REPORTING (MISSING - ADD THESE) =============

        /// <summary>
        /// Customer reports consultant no-show. Full refund to customer.
        /// </summary>
        Task<ConsultationResponse> ReportConsultantNoShowAsync(Guid consultationId);

        /// <summary>
        /// Consultant reports customer no-show. 50% to consultant, 50% refunded to customer.
        /// </summary>
        Task<ConsultationResponse> ReportCustomerNoShowAsync(Guid consultationId);

        // ============= QUERIES =============

        /// <summary>
        /// Customer retrieves all their consultations with escrow status.
        /// </summary>
        Task<IEnumerable<ConsultationResponse>> GetMyConsultationsAsync();

        /// <summary>
        /// Consultant retrieves all their consultations with pending payouts.
        /// </summary>
        Task<IEnumerable<ConsultationResponse>> GetConsultantConsultationsAsync();
    }
}