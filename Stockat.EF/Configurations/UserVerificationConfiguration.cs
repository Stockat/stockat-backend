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
        builder
             .HasOne(uv => uv.User)
             .WithOne(u => u.UserVerification)
             .HasForeignKey<UserVerification>(uv => uv.UserId)
             .OnDelete(DeleteBehavior.Cascade);
    }
}
