using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.Shared.DTO_s.Response
{
    public class ConsultationResponse
    {
        public Guid Id { get; set; }

        // Who booked
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }

        
        public string ConsultantId { get; set; }
        public string ConsultantName { get; set; }

       
        public int? ServiceId { get; set; }
        public string ServiceName { get; set; }

       
        public string Notes { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Status { get; set; }

        
        public string SendbirdChannelUrl { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
