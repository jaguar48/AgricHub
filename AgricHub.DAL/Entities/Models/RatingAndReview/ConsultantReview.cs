using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgricHub.DAL.Entities.Models.RatingAndReview;

public class ConsultantReview
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Consultant))]
    public int ConsultantId { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(2000)]
    public string? Comment { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(Consultation))]
    public int? ConsultationId { get; set; }

    public bool IsHidden { get; set; } = false;

    // Navigation properties
    public virtual Consultant? Consultant { get; set; }
    public virtual ApplicationUser? User { get; set; }
    public virtual Consultation? Consultation { get; set; }
}
