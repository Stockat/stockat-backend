using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockat.Core.Entities;

namespace Stockat.EF.Configurations
{
    public class RequestProductConfiguration : IEntityTypeConfiguration<RequestProduct>
    {
        public void Configure(EntityTypeBuilder<RequestProduct> builder)
        {
            // Optional: configure Description field constraints at the database level
            builder.Property(rp => rp.Description)
                .IsRequired()
                .HasMaxLength(250);

            // If you want to use TPH (default), no need to set table name.
            // If you want a separate table (TPT), uncomment the following:
            // builder.ToTable("RequestProducts");
        }
    }
}
