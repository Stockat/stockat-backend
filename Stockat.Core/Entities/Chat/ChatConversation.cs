using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stockat.Core.Entities.Chat;

public class ChatConversation
{
    [Key]
    public int ConversationId { get; set; }

    [Required]
    public string User1Id { get; set; }

    [Required]
    public string User2Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastMessageAt { get; set; }

    public bool IsActive { get; set; } = true;

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("User1Id")]
    public virtual User User1 { get; set; }

    [ForeignKey("User2Id")]
    public virtual User User2 { get; set; }

    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
