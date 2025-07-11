using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockat.Core.Entities;

namespace Stockat.EF.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Rating)
                .IsRequired()
                .HasMaxLength(1);

            builder.Property(r => r.Comment)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(r => r.ReviewerId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(r => r.Reviewer)
                .WithMany()
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            builder.HasOne(r => r.Service)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.ServiceId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasOne(r => r.OrderProduct)
                .WithMany(op => op.Reviews)
                .HasForeignKey(r => r.OrderProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            builder.HasOne(r => r.ServiceRequest)
                .WithMany(sr => sr.Reviews)
                .HasForeignKey(r => r.ServiceRequestId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Indexes
            builder.HasIndex(r => r.ReviewerId);
            builder.HasIndex(r => r.ProductId);
            builder.HasIndex(r => r.ServiceId);
            builder.HasIndex(r => r.OrderProductId);
            builder.HasIndex(r => r.ServiceRequestId);
            builder.HasIndex(r => r.CreatedAt);

            // Check constraint: Either ProductId or ServiceId must be provided, but not both
            builder.HasCheckConstraint("CK_Review_ProductOrService", 
                "(ProductId IS NOT NULL AND ServiceId IS NULL) OR (ProductId IS NULL AND ServiceId IS NOT NULL)");
        }
    }
} 