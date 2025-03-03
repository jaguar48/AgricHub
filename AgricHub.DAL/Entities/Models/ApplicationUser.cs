using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.DAL.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string CountryId { get; set; }
        public required string StateId { get; set; }
        public required string Address { get; set; }
        public string? VerificationToken { get; set; }
    }
}
