using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockat.Core.Entities;

namespace Stockat.EF.Configurations;

public class ServiceEditRequestConfiguration : IEntityTypeConfiguration<ServiceEditRequest>
{
    public void Configure(EntityTypeBuilder<ServiceEditRequest> builder)
    {
        builder.Property(e => e.ApprovalStatus).HasConversion<string>();
    }
}
