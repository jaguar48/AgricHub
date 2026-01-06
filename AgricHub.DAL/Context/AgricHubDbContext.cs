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

            // ============= CONSULTATION RELATIONSHIPS =============

            // Consultation → Customer
            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Customer)
                .WithMany()
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Consultation → Consultant
            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Consultant)
                .WithMany()
                .HasForeignKey(c => c.ConsultantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Consultation → Service (optional)
            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Service)
                .WithMany()
                .HasForeignKey(c => c.ServiceId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Consultation → ServicePackage (optional)
            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.ServicePackage)
                .WithMany()
                .HasForeignKey(c => c.ServicePackageId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // ============= CHAT SESSION RELATIONSHIPS =============

            // ChatSession → Customer
            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.Customer)
                .WithMany()
                .HasForeignKey(cs => cs.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ChatSession → Consultant
            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.Consultant)
                .WithMany()
                .HasForeignKey(cs => cs.ConsultantId)
                .OnDelete(DeleteBehavior.Restrict);

            // ChatSession → Service (optional)
            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.Service)
                .WithMany()
                .HasForeignKey(cs => cs.ServiceId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // ============= REVIEW RELATIONSHIPS =============

            // ============= REVIEW RELATIONSHIPS =============

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Consultation)
                .WithOne()
                .HasForeignKey<Review>(r => r.ConsultationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Consultant)
                .WithMany()
                .HasForeignKey(r => r.ConsultantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Service)
                .WithMany()
                .HasForeignKey(r => r.ServiceId)
                .IsRequired(false)  // ← ADD THIS LINE!
                .OnDelete(DeleteBehavior.SetNull);

            // ============= WALLET RELATIONSHIPS =============

            // Wallet → Customer (nullable)
            modelBuilder.Entity<Wallet>()
                .HasOne(w => w.Customer)
                .WithMany()
                .HasForeignKey(w => w.CustomerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Wallet → Consultant (nullable)
            modelBuilder.Entity<Wallet>()
                .HasOne(w => w.Consultant)
                .WithMany()
                .HasForeignKey(w => w.ConsultantId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Check constraint: Either CustomerId or ConsultantId must be set, but not both
            modelBuilder.Entity<Wallet>()
                .HasCheckConstraint("CK_Wallet_CustomerOrConsultant",
                    "(CustomerId IS NOT NULL AND ConsultantId IS NULL) OR (CustomerId IS NULL AND ConsultantId IS NOT NULL)");

            // ============= WALLET TRANSACTION RELATIONSHIPS =============

            // WalletTransaction → Customer (nullable)
            modelBuilder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Customer)
                .WithMany(c => c.WalletTransactions)
                .HasForeignKey(wt => wt.CustomerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // WalletTransaction → Consultant (nullable)
            modelBuilder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Consultant)
                .WithMany(c => c.WalletTransactions)
                .HasForeignKey(wt => wt.ConsultantId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // ============= PENDING TRANSACTION RELATIONSHIPS =============

            // PendingTransaction → Customer
            modelBuilder.Entity<PendingTransaction>()
                .HasOne(pt => pt.Customer)
                .WithMany()
                .HasForeignKey(pt => pt.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // PendingTransaction → Consultation
            modelBuilder.Entity<PendingTransaction>()
                .HasOne(pt => pt.Consultation)
                .WithMany()
                .HasForeignKey(pt => pt.ConsultationId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============= SERVICE PACKAGE RELATIONSHIPS =============

            // ServicePackage → Service
            modelBuilder.Entity<ServicePackage>()
                .HasOne(sp => sp.Service)
                .WithMany(s => s.Packages)
                .HasForeignKey(sp => sp.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============= CUSTOM OFFER RELATIONSHIPS =============

            // CustomOffer → ChatSession
            modelBuilder.Entity<CustomOffer>()
                .HasOne(co => co.ChatSession)
                .WithMany()
                .HasForeignKey(co => co.ChatSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // CustomOffer → Service
            modelBuilder.Entity<CustomOffer>()
                .HasOne(co => co.Service)
                .WithMany()
                .HasForeignKey(co => co.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============= SERVICE RELATIONSHIPS =============

            // Service → Business
            modelBuilder.Entity<Service>()
                .HasOne(s => s.Business)
                .WithMany()
                .HasForeignKey(s => s.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            // Service → Category
            modelBuilder.Entity<Service>()
                .HasOne(s => s.Category)
                .WithMany()
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============= BUSINESS RELATIONSHIPS =============

            // Business → Consultant
            modelBuilder.Entity<Business>()
                .HasOne(b => b.Consultant)
                .WithMany()
                .HasForeignKey(b => b.ConsultantId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============= DECIMAL PRECISION CONFIGURATIONS =============

            // Ensure proper decimal precision for financial fields
            modelBuilder.Entity<Wallet>()
                .Property(w => w.Balance)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<WalletTransaction>()
                .Property(wt => wt.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PendingTransaction>()
                .Property(pt => pt.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ServicePackage>()
                .Property(sp => sp.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Service>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CustomOffer>()
                .Property(co => co.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Consultation>()
                .Property(c => c.CustomPrice)
                .HasColumnType("decimal(18,2)");
        }

        // ============= DBSETS =============

        public DbSet<Consultant> Consultants { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServicePackage> ServicePackages { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<PendingTransaction> PendingTransactions { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Consultation> Consultations { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<CustomOffer> CustomOffers { get; set; }
    }
}