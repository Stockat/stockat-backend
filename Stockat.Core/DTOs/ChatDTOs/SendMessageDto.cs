using System.ComponentModel.DataAnnotations;

namespace Stockat.Core.DTOs.ChatDTOs;

public class SendMessageDto
{
    [Required]
    public int ConversationId { get; set; }

    [MaxLength(2000)]
    public string? MessageText { get; set; }
}