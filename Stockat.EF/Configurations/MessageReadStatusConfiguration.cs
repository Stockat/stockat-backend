using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockat.Core.Entities.Chat;

namespace Stockat.EF.Configurations;

public class MessageReadStatusConfiguration : IEntityTypeConfiguration<MessageReadStatus>
{
    public void Configure(EntityTypeBuilder<MessageReadStatus> builder)
    {
        builder.HasKey(r => r.ReadStatusId);

        builder.Property(r => r.ReadAt)
               .HasColumnType("datetime");

        builder.HasOne(r => r.Message)
               .WithMany(m => m.ReadStatuses)
               .HasForeignKey(r => r.MessageId)
               .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to avoid multiple cascade paths

        builder.HasOne(r => r.User)
               .WithMany(u => u.MessageReadStatuses)
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
