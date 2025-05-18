using AgricHub.DAL.Entities;
using AgricHub.Shared.DTO_s.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.BLL.Interfaces.IBusinessServices
{
    
    public interface IConsultationService
    {
        Task<Consultation> BookConsultationAsync(ConsultationBookingRequest dto);
    }

}
