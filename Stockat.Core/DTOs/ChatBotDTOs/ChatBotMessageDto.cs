using System;

namespace Stockat.Core.DTOs.ChatBotDTOs;

public class ChatBotMessageDto
{
    public string MessageText { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
} 