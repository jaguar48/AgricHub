using System;
using AgricHub.DAL.Entities.Models.RatingAndReview;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgricHub.DAL.Context;

public class ReviewReportConfiguration: IEntityTypeConfiguration<ReviewReport>
    {
        public void Configure(EntityTypeBuilder<ReviewReport> builder)
        {
            builder.ToTable("ReviewReports");
            
            builder.HasIndex(r => new { r.ReportingUserId, r.ReviewId });
            
            builder.Property(r => r.ReportedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }