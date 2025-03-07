using System;
using AgricHub.Shared.DTO_s.Response;
using AgricHub.Shared.DTO_s.Response.RatingAndReview;
using AgricHub.Shared.Enums.RatingAndReview;

namespace AgricHub.BLL.Interfaces.RatingAndReview;

public interface IReviewModeration
{
    Task<OperationResult<int>> FlagReviewAsync(int reviewId, string reportingUserId, 
        ReportReason reason, string? details = null);
        
    Task<OperationResult> UpdateReportStatusAsync(int reportId, ReportStatus newStatus,
        string moderatorUserId);
        
    Task<PaginatedResult<ReviewReportResponse>> GetPendingReportsAsync(int pageSize = 50, 
        DateTime? olderThan = null);
        
    Task<ReviewReportResponse?> GetReportDetailsAsync(int reportId);
}
