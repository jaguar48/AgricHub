using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.DAL.Entities
{
    public class Service
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } 
        public string Description { get; set; }
        public decimal Price { get; set; } 
        public bool IsAvailable { get; set; } = true;
        public string? ImagePath { get; set; }

        public int BusinessId { get; set; }
        public virtual Business Business { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }

}
