using AgricHub.Shared.Enums.RatingAndReview;

namespace AgricHub.Shared.DTO_s.Response.RatingAndReview;
public record ReviewReportResponse(
    int Id,
    int ReviewId,
    string ReportingUserId,
    ReportReason Reason,
    string? Details,
    ReportStatus Status,
    DateTime ReportedAt,
    DateTime? ResolvedAt,
    string? ResolvedByUserId
);
