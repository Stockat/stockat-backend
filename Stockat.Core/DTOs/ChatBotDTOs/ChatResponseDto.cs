using Stockat.Core.DTOs.ServiceDTOs;

namespace Stockat.Core.DTOs.ChatBotDTOs;

public class ChatResponseDto
{
    public string Response { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<ServiceSuggestionDto>? ServiceSuggestions { get; set; }
    public string UserIntent { get; set; } = string.Empty;
    public bool IsAIEnhanced { get; set; } = false;
    public Dictionary<string, object>? Metadata { get; set; }
} 