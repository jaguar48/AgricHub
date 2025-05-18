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
        public int ConsultantId { get; set; }

        public DateTime ScheduledAt { get; set; }
        public string Status { get; set; } = "Pending"; // or Confirmed, Cancelled, etc.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? SendbirdChannelUrl { get; set; }
    }

}
