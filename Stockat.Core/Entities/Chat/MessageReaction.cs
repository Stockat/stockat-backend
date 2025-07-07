using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stockat.Core.Entities.Chat;

public class MessageReaction
{
    [Key]
    public int ReactionId { get; set; }

    [Required]
    public int MessageId { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    [StringLength(10)]
    public string ReactionType { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("MessageId")]
    public virtual ChatMessage Message { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}
