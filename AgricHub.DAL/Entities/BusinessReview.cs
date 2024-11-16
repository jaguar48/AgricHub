using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.DAL.Entities
{
    public class BusinessReview
    {
        public int Id { get; set; }

        
        
        public string UserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime DateCreated { get; set; }


        public virtual Business Business { get; set; }
        public ApplicationUser User { get; set; }
    }
}
