using AgricHub.DAL.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.DAL.Entities
{
    public class Consultation
    {
        public Guid Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public int ConsultantId { get; set; }
        public Consultant Consultant { get; set; }
        public int? ServiceId { get; set; }
        public Service Service { get; set; }
        public int? ServicePackageId { get; set; }
        public ServicePackage ServicePackage { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime EndAt { get; set; }
        public string Status { get; set; }
        public string SendbirdChannelUrl { get; set; }
        public string? DeliverablesPath { get; set; }  // ← CHANGED: Added "?"
        public bool ConsultantNoShowReported { get; set; }
        public bool CustomerNoShowReported { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }  // ← ADD THIS (for tracking completion)

        // Custom offer properties
        public bool IsCustomOffer { get; set; }
        public decimal? CustomPrice { get; set; }
        public int? CustomDurationMinutes { get; set; }
    }
}