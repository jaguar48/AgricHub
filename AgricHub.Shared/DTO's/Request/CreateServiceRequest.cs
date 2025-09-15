using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AgricHub.Shared.DTO_s.Request
{




    public class CreateServiceRequest
    {
        public int BusinessId { get; set; }
        public int CategoryId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public IFormFile File { get; set; }

        // Swagger will treat this as a simple string field
        public string PackagesJson { get; set; }

        [NotMapped]
        [JsonIgnore]
        [BindNever]
        [ValidateNever]
        public List<ServicePackageRequest> Packages { get; set; }
    }

    public class ServicePackageRequest
    {
        public int Id { get; set; } // For updates
        public string PackageName { get; set; } // e.g., Basic, Standard, Consulting, Premium
        public decimal Price { get; set; }
        public string Description { get; set; }
        public bool IncludesOnsiteVisit { get; set; }
    }
}
