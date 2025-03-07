using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgricHub.DAL.Entities.Models.RatingAndReview;

namespace AgricHub.DAL.Entities.Models
{
    public class Consultant
    {
       
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string BusinessName { get; set; }
        public required string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string ? UserId { get; set; }
        public string ? CountryId { get; set; }
        public bool IsVerified { get; set; } = false;
        public string ? StateId { get; set; }
        public string ? Address { get; set; }
        public ApplicationUser User { get; set; }



         // Aggregated values for quick access
        public double? AverageRating { get; set; }
        public int? TotalReviews { get; set; }
        
        // Navigation properties
        public virtual ICollection<ConsultantReview>? Reviews { get; set; }
        public virtual ICollection<Consultation>? Consultations { get; set; }
    }
}
