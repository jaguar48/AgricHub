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
        public int ServiceId { get; set; }
        public Service Service { get; set; }
        public int? ServicePackageId { get; set; } // New field
        public ServicePackage ServicePackage { get; set; }
        public string? SendbirdChannelUrl { get; set; }
        public string Status { get; set; } // e.g., Pending, Approved, In Progress, Completed, Rejected, Cancelled
        public DateTime ScheduledAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Notes { get; set; }
    }



}
