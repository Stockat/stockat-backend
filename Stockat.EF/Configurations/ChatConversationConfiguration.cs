using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockat.Core.Entities.Chat;

namespace Stockat.EF.Configurations;

public class ChatConversationConfiguration : IEntityTypeConfiguration<ChatConversation>
{
    public void Configure(EntityTypeBuilder<ChatConversation> builder)
    {
        builder.HasKey(c => c.ConversationId);

        builder.HasOne(c => c.User1)
               .WithMany(u => u.ConversationsAsUser1)
               .HasForeignKey(c => c.User1Id)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.User2)
               .WithMany(u => u.ConversationsAsUser2)
               .HasForeignKey(c => c.User2Id)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.CreatedAt)
               .HasColumnType("datetime");
    }
}
