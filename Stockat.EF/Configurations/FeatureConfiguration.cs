using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.EF.Configurations;

public class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder
             .HasMany(f => f.FeatureValues)
             .WithOne(fv => fv.Feature)
             .HasForeignKey(f => f.FeatureId);
        //.OnDelete(DeleteBehavior.Cascade);


        builder
            .HasMany(sd => sd.StockDetails)
            .WithOne(f => f.Feature)
            .HasForeignKey(p => p.FeatureId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
