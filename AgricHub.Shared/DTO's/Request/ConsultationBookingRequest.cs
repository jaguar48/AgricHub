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
        public DateTime ScheduledAt { get; set; }
    }

}
