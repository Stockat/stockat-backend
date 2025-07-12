using System.Threading.Tasks;
using Stockat.Core.DTOs.ChatBotDTOs;

namespace Stockat.Core.IServices;

public interface IOpenAIService
{
    Task<string> GenerateResponseAsync(string userMessage, object contextData);
    Task<string> GenerateResponseWithHistoryAsync(string userMessage, IEnumerable<ChatBotMessageDto> chatHistory, object contextData);
    Task<string> GenerateResponseWithModelAsync(string userMessage, object contextData, string model);
} 