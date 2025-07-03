using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockat.Core.Entities.Chat;

namespace Stockat.EF.Configurations;

public class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
{
    public void Configure(EntityTypeBuilder<MessageReaction> builder)
    {
        builder.HasKey(r => r.ReactionId);

        builder.Property(r => r.ReactionType)
               .HasMaxLength(10)
               .IsRequired();

        builder.Property(r => r.CreatedAt)
               .HasColumnType("datetime");

        builder.HasOne(r => r.Message)
               .WithMany(m => m.Reactions)
               .HasForeignKey(r => r.MessageId)
               .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to avoid multiple cascade paths

        builder.HasOne(r => r.User)
               .WithMany(u => u.MessageReactions)
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
