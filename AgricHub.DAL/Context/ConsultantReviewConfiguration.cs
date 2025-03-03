using System;
using AgricHub.DAL.Entities.Models.RatingAndReview;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgricHub.DAL.Context;

public class ConsultantReviewConfiguration : IEntityTypeConfiguration<ConsultantReview>
    {
        public void Configure(EntityTypeBuilder<ConsultantReview> builder)
        {
            builder.ToTable("ConsultantReviews");
            
            builder.HasIndex(r => new { r.ConsultantId, r.CreatedDate });
            builder.HasIndex(r => r.UserId);
            
            builder.Property(r => r.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
