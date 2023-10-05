using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.DAL.Entities.Models
{
    public class Consultant
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BusinessName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string ? UserId { get; set; }
        public string ? CountryId { get; set; }
        public string ? StateId { get; set; }
        public string ? Address { get; set; }
        public ApplicationUser User { get; set; }
    }
}
