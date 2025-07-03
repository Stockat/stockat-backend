using Stockat.Core.Entities;
using Stockat.Core.Entities.Chat;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stockat.Core.Entities.Chat;
public class MessageReadStatus
{
    [Key]
    public int MessageId { get; set; } // PK and FK to ChatMessage

    [Required]
    public string UserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(MessageId))]
    public virtual ChatMessage Message { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }
}
