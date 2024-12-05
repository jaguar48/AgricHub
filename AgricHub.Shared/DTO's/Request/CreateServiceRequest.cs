using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.Shared.DTO_s.Request
{
   
    public class CreateServiceRequest
    {
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public IFormFile? File { get; set; }
        public int BusinessId { get; set; } // Link the service to a business
    }
}
