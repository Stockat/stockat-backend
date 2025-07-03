using System;
using System.ComponentModel.DataAnnotations;

namespace Stockat.Core.DTOs.ChatDTOs;

public class MessageReactionDto
{
    [Required]
    public int ReactionId { get; set; }

    [Required]
    public int MessageId { get; set; }

    [Required]
    [StringLength(50)]
    public string UserId { get; set; }

    [Required]
    [StringLength(20)]
    public string ReactionType { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }
}