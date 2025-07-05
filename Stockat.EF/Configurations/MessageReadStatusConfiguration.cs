using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockat.Core.Entities.Chat;

namespace Stockat.EF.Configurations;

public class MessageReadStatusConfiguration : IEntityTypeConfiguration<MessageReadStatus>
{
    public void Configure(EntityTypeBuilder<MessageReadStatus> builder)
    {
        builder.HasKey(r => r.MessageId); // MessageId is both PK and FK (1-to-1)

        builder.Property(r => r.ReadAt)
               .HasColumnType("datetime");

        builder.HasOne(r => r.Message)
               .WithOne(m => m.ReadStatus)
               .HasForeignKey<MessageReadStatus>(r => r.MessageId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.User)
               .WithMany(u => u.MessageReadStatuses) // assuming User has a collection
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.UserId, r.MessageId });

    }
}

