using Stockat.Core.Entities;
using Stockat.Core.Entities.Chat;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stockat.Core.Entities.Chat;
public class ChatMessage
{
    [Key]
    public int MessageId { get; set; }

    [Required]
    public int ConversationId { get; set; }

    [Required]
    public string SenderId { get; set; }

    public string? MessageText { get; set; }

    public string? ImageUrl { get; set; }
    public string? ImageId { get; set; }

    public string? VoiceUrl { get; set; }
    public string? VoiceId { get; set; }

    public bool IsEdited { get; set; } = false;
    public bool IsRead { get; set; } = false;

    [Column(TypeName = "datetime")]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ConversationId")]
    public virtual ChatConversation Conversation { get; set; }

    [ForeignKey("SenderId")]
    public virtual User Sender { get; set; }

    public virtual ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();

    public virtual MessageReadStatus ReadStatus { get; set; } // 1-to-1
}
