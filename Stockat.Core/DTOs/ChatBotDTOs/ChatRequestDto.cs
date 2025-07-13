namespace Stockat.Core.DTOs.ChatBotDTOs;

public class ChatRequestDto
{
    public string? Message { get; set; }
    public bool IncludeServiceSuggestions { get; set; } = false;
    public object? AdditionalContext { get; set; }
} 