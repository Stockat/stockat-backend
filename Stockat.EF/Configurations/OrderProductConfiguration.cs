using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.EF.Configurations;

public class OrderProductConfiguration : IEntityTypeConfiguration<OrderProduct>
{
    public void Configure(EntityTypeBuilder<OrderProduct> builder)
    {

        builder
            .HasOne(p => p.Seller)
            .WithMany(s => s.SellerOrderProducts)
            .HasForeignKey(p => p.SellerId)
        .OnDelete(DeleteBehavior.NoAction);

        builder
           .HasOne(p => p.Buyer)
           .WithMany(s => s.BuyerOrderProducts)
           .HasForeignKey(p => p.BuyerId)
           .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(d => d.Driver)
            .WithOne(o => o.AssignedOrder)
            .HasForeignKey<OrderProduct>(d => d.DriverId)
            .OnDelete(DeleteBehavior.NoAction);


        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);

    }
}
