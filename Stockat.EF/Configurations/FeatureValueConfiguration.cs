using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.EF.Configurations;

public class FeatureValueConfiguration : IEntityTypeConfiguration<FeatureValue>
{
    public void Configure(EntityTypeBuilder<FeatureValue> builder)
    {
        builder
             .HasMany(sd => sd.StockDetails)
             .WithOne(fv => fv.FeatureValue)
             .HasForeignKey(p => p.FeatureValueId)
             .OnDelete(DeleteBehavior.Restrict);

    }
}
