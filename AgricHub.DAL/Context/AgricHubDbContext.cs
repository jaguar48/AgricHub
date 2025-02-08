using AgricHub.DAL.Entities;
using AgricHub.DAL.Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

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
            modelBuilder.Entity<Business>()
        .HasOne(b => b.Category)
        .WithMany()
        .HasForeignKey(b => b.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Category>()
       .HasMany(c => c.Businesses)
       .WithOne(b => b.Category)
       .HasForeignKey(b => b.CategoryId)
       .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);

            /*modelBuilder.ApplyConfiguration(new RoleConfiguration());*/


        }


        public DbSet<Consultant> Consultants { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Category> categories { get; set; }

    }
}
