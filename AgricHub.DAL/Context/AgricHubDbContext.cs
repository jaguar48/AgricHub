using AgricHub.DAL.Entities;
using AgricHub.DAL.Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgricHub.DAL.Context
{
    public class AgricHubDbContext : IdentityDbContext<ApplicationUser>
    {
        
            public AgricHubDbContext(DbContextOptions<AgricHubDbContext> options)
                : base(options)
            {
            }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

           

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new RoleConfiguration());


        }


        public DbSet<Consultant> Consultants { get; set; }

    }
}
