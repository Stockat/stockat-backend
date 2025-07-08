using System;

namespace Stockat.Core.DTOs.ChatDTOs;

/// <summary>
/// Simplified DTO for chatbot messages
/// </summary>
public class ChatBotMessageDto
{
    public string MessageText { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public string SenderId { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
} 