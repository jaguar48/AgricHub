using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.Shared.DTO_s.Request
{


    public class ConsultationBookingRequest
    {
        public string ConsultantId { get; set; }
        public int ServiceId { get; set; }
        public int ServicePackageId { get; set; } // Required for purchases
        public DateTime ScheduledAt { get; set; }
        public string? Notes { get; set; }
    }


}
