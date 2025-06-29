using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.EF.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder
            .HasOne(u => u.User)
            .WithMany(u => u.Products)
            .HasForeignKey(p => p.SellerId);

        builder
            .HasOne(u => u.Category)
            .WithMany(u => u.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);


        builder
            .HasMany(p => p.OrderProducts)
            .WithOne(u => u.Product)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasMany(p => p.ProductTags)
            .WithOne(u => u.Product)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.ProductStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.Location).HasConversion<string>().HasMaxLength(40);



    }
}
