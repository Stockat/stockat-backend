namespace Stockat.Core.DTOs.ChatBotDTOs;

public class ServiceSuggestionDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
} 