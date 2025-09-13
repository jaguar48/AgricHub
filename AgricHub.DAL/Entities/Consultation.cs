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
        public Guid Id { get; set; } = Guid.NewGuid();

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }   // ✅ Navigation

        public int ConsultantId { get; set; }
        public Consultant Consultant { get; set; }  // ✅ Navigation

        public int? ServiceId { get; set; }
        public Service Service { get; set; }    // ✅ Navigation

        public string? Notes { get; set; }

        public DateTime ScheduledAt { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? SendbirdChannelUrl { get; set; }
    }



}
