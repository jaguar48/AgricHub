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

    
        public string CustomerUserId { get; set; }     
        public string CustomerName { get; set; }

      
        public string ConsultantUserId { get; set; }
        public string ConsultantName { get; set; }

        public int? ServiceId { get; set; }
        public string ServiceName { get; set; }
        public int? ServicePackageId { get; set; }
        public string PackageName { get; set; }
        public string Notes { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime EndAt { get; set; }
        public string Status { get; set; }
        public string SendbirdChannelUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCustomOffer { get; set; }
        public decimal? CustomPrice { get; set; }
        public int? CustomDurationMinutes { get; set; }
        public decimal PendingAmount { get; set; }
    }
}