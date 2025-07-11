using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stockat.Core.DTOs.ChatDTOs;

/// <summary>
/// Represents a chat message, including sender info and reactions.
/// </summary>
public class ChatMessageDto
{
    public int MessageId { get; set; }
    public int ConversationId { get; set; }
    public UserChatInfoDto Sender { get; set; }
    public string? MessageText { get; set; }
    public string? ImageUrl { get; set; }
    public string? VoiceUrl { get; set; }
    public bool IsEdited { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public List<MessageReactionDto> Reactions { get; set; } = new();
    
    // For chatbot messages
    public string? Role { get; set; } // "user" or "assistant"
    public string? SenderId { get; set; } // For chatbot messages that don't have a full User object
}