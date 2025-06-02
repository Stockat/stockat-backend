using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.EF.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        // seeding
        builder.HasData(
            new IdentityRole
            {
                Id = "1", 
                Name = "Admin",
                NormalizedName = "ADMIN"
            },
            new IdentityRole
            {
                Id = "2",
                Name = "Buyer",
                NormalizedName = "BUYER"
            },
            new IdentityRole
            {
                Id = "3",
                Name = "Seller",
                NormalizedName = "SELLER"
            }
        );
    }

}
