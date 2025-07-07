using System.ComponentModel.DataAnnotations;

namespace Stockat.Core.DTOs.ChatDTOs;

public class ReactToMessageDto
{
    [Required]
    public int ConversationId { get; set; }
    [Required]
    public int MessageId { get; set; }

    [Required]
    [MaxLength(10)]
    public string ReactionType { get; set; }

}