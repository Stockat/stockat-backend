using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.EF.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder
            .HasOne(p => p.Product)
            .WithMany(s => s.Stocks)
            .HasForeignKey(p => p.ProductId);
        //.OnDelete(DeleteBehavior.Cascade); // Optional: cascade delete Stocks with product


        builder
            .HasMany(sd => sd.StockDetails)
            .WithOne(s => s.Stock)
            .HasForeignKey(p => p.StockId)
            .OnDelete(DeleteBehavior.Restrict);


    }
}
