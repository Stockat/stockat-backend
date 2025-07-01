using System;
using System.Collections.Generic;

namespace Stockat.Core.DTOs.ChatDTOs;

/// <summary>
/// Represents a chat conversation, including participants, messages, and the last message.
/// </summary>
public class ChatConversationDto
{
    public int ConversationId { get; set; }
    public UserChatInfoDto User1 { get; set; }
    public UserChatInfoDto User2 { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = new();

    /// <summary>
    /// The last message in the conversation (for preview in conversation list).
    /// </summary>
    public ChatMessageDto LastMessage { get; set; }
}