using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using Stockat.Core.Enums;

namespace Stockat.EF.Configurations;

public class UserPunishmentConfiguration : IEntityTypeConfiguration<UserPunishment>
{
    public void Configure(EntityTypeBuilder<UserPunishment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
              .ValueGeneratedOnAdd();

        builder.Property(e => e.Type)
              .HasConversion<string>()
              .IsRequired();

        builder.Property(e => e.Reason)
              .IsRequired()
              .HasMaxLength(500);

        builder.Property(e => e.StartDate)
              .IsRequired();

        builder.Property(e => e.CreatedAt)
              .IsRequired();

        // Foreign key relationship with User
        builder.HasOne(e => e.User)
              .WithMany(u => u.Punishments)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);

        // Index for better performance when querying by user
        builder.HasIndex(e => e.UserId);

        // Index for querying active punishments
        builder.HasIndex(e => new { e.Type, e.EndDate });
    }
} 