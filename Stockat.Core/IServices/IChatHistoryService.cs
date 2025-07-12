using System.Threading.Tasks;
using Stockat.Core.DTOs.ChatBotDTOs;

namespace Stockat.Core.IServices;

public interface IChatHistoryService
{
    Task SaveMessageAsync(string userId, string role, string message);
    Task<IEnumerable<ChatBotMessageDto>> GetChatHistoryAsync(string userId, int limit = 50);
    Task ClearChatHistoryAsync(string userId);
    Task SaveChatBotMessageAsync(string userId, string role, string message);
    Task<IEnumerable<ChatBotMessageDto>> GetChatBotHistoryAsync(string userId, int limit = 50);
    Task ClearChatBotHistoryAsync(string userId);
} 