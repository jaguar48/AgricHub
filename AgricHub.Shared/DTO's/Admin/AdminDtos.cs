using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.Shared.DTO_s
{ 

    // ── Overview ─────────────────────────────────────────────
    public record AdminStatsDto(
        int TotalUsers,
        int VerifiedConsultants,
        int PendingVerifications,
        int CompletedSessions30d,
        int TotalReviews,
        IReadOnlyList<RecentReviewSummaryDto> RecentReviews,
        IReadOnlyList<PendingVerifSummaryDto> PendingVerifs
    );

    public record RecentReviewSummaryDto(
        int Id,
        string CustomerName,
        string ConsultantName,
        int Rating,
        string? Comment,
        DateTime CreatedAt
    );

    public record PendingVerifSummaryDto(
        int Id,
        string Name,
        string BusinessName,
        string? CountryId
    );

    // ── Reviews (Moderation) ──────────────────────────────────
    public record AdminReviewDto(
        int Id,
        Guid ConsultationId,
        string CustomerName,
        string ConsultantName,
        int Rating,
        string? Comment,
        DateTime CreatedAt
    );

    // ── Verifications ─────────────────────────────────────────
    public record VerificationDto(
        int Id,
        string FirstName,
        string LastName,
        string BusinessName,
        string Email,
        string? PhoneNumber,
        string? CountryId,
        string? StateId,
        bool IsVerified,
        string Status,
        string? SubmittedAt,
        string? RejectionNotes,
        List<string> Documents
    );

    public record UpdateVerificationRequest(bool Approve, string? Notes = null);

    // ── Users ─────────────────────────────────────────────────
    public record AdminUserDto(
        string Id,
        string FirstName,
        string LastName,
        string Email,
        string? CountryId,
        IList<string> Roles
    );

    public record AdminUserPagedResult(
        IReadOnlyList<AdminUserDto> Items,
        int Total,
        int Page,
        int PageSize,
        int TotalCustomers,
        int TotalConsultants,
        int TotalAdmins
    );

    // ── Consultants ───────────────────────────────────────────
    public record AdminConsultantDto(
        int Id,
        string FirstName,
        string LastName,
        string? BusinessName,
        string? CountryId,
        string? StateId,
        bool IsVerified,
        int CompletedConsultations,
        double AverageRating,
        int TotalReviews
    );

    public record AdminConsultantPagedResult(
        IReadOnlyList<AdminConsultantDto> Items,
        int Total,
        int Page,
        int PageSize
    );

    // ── Categories ────────────────────────────────────────────
    public record CategoryDto(
        int Id,
        string Name,
        int ServiceCount
    );

    // CreateCategoryRequest intentionally removed —
    // use the existing AgricHub.Shared.DTO_s.Request.CreateCategoryRequest
}