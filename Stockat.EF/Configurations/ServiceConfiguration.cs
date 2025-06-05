using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using System.Reflection.Emit;

namespace Stockat.EF.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        // User - Service (1:N)
        builder
            .HasOne(s => s.Seller)
            .WithMany(u => u.Services)
            .HasForeignKey(s => s.SellerId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}

