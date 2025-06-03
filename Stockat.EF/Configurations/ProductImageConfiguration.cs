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

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder
       .HasOne(pi => pi.Product)
       .WithMany(p => p.Images)
       .HasForeignKey(pi => pi.ProductId)
       .OnDelete(DeleteBehavior.Cascade); // Optional: cascade delete images with product
    }
}
