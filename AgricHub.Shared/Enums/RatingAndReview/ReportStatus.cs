namespace AgricHub.Shared.Enums.RatingAndReview;

public enum ReportStatus
{
    Pending,          // New unreviewed report
    UnderReview,      // Actively being reviewed
    ResolvedApproved, // Report was valid - content moderated
    ResolvedRejected  // Report was invalid - no action taken
}
