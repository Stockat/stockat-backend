using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Stockat.Core.DTOs.ChatDTOs;

public class SendImageMessageDto
{
    [Required]
    public int ConversationId { get; set; }

    [MaxLength(2000)]
    public string? MessageText { get; set; }

    [Required]
    public IFormFile Image { get; set; } = null!;
}
