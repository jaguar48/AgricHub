using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgricHub.Shared.Enums.RatingAndReview;

namespace AgricHub.DAL.Entities.Models.RatingAndReview;

public class ReviewReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Review))]
        public int ReviewId { get; set; }

        [Required]
        [ForeignKey(nameof(ReportingUser))]
        public int ReportingUserId { get; set; }

        [Required]
        public ReportReason Reason { get; set; }

        [StringLength(500)]
        public string? Details { get; set; }

        [Required]
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        [Required]
        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        [ForeignKey(nameof(ResolvedByUser))]
        public int? ResolvedByUserId { get; set; }

        // Navigation properties
        public virtual ConsultantReview? Review { get; set; }
        public virtual ApplicationUser? ReportingUser { get; set; }
        public virtual ApplicationUser? ResolvedByUser { get; set; }
    }
