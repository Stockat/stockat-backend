using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using System.Reflection.Emit;

namespace Stockat.EF.Configurations;

public class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
{
    public void Configure(EntityTypeBuilder<ServiceRequest> builder)
    {

        // Service - ServiceRequest (1:N)
        builder
            .HasOne(sr => sr.Service)
            .WithMany()
            .HasForeignKey(sr => sr.ServiceId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
          .HasOne(sr => sr.Buyer)
          .WithMany()
          .HasForeignKey(sr => sr.BuyerId)
          .OnDelete(DeleteBehavior.NoAction);

        builder
          .Property(s => s.SellerApprovalStatus).HasConversion<string>();
        builder
            .Property(s => s.BuyerApprovalStatus).HasConversion<string>();
        builder
            .Property(e => e.PaymentStatus).HasConversion<string>();
        builder
            .Property(e => e.ServiceStatus).HasConversion<string>();
    }
}

