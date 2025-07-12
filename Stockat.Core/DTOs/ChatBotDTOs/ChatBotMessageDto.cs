namespace Stockat.Core.DTOs.ChatBotDTOs;

public class ChatBotMessageDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public DateTime Timestamp { get; set; }
    public bool IsAIEnhanced { get; set; } = false;
    public string? UserIntent { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
} 