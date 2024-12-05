using AgricHub.Shared.DTO_s.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.BLL.Interfaces.IAgrichub_Services
{
    public interface IBusinessForService
    {

        Task<string> AddService(CreateServiceRequest serviceRequest);
    }
}
