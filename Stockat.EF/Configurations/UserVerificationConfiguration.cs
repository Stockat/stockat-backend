using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.EF.Configurations;

public class UserVerificationConfiguration: IEntityTypeConfiguration<UserVerification>
{
    public void Configure(EntityTypeBuilder<UserVerification> builder)
    {
        builder.HasKey(e => e.UserId); // use UserId as pk and fk

        builder.Property(e => e.Status)
              .HasConversion<string>(); 

        builder.HasOne(e => e.User)
              .WithOne(u => u.UserVerification)
              .HasForeignKey<UserVerification>(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
