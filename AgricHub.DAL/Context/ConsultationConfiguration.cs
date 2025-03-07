using System;
using AgricHub.DAL.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgricHub.DAL.Context;

 public class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
    {
        public void Configure(EntityTypeBuilder<Consultation> builder)
        {
            builder.ToTable("Consultations");
            
            builder.HasIndex(c => new { c.ConsultantId, c.UserId });
            builder.HasIndex(c => c.EndDate);
        }
    }
