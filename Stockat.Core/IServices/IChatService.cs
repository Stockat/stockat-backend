using Microsoft.AspNetCore.Http;
using Stockat.Core.DTOs.ChatDTOs;
using Stockat.Core.Entities.Chat;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stockat.Core.IServices;

/// <summary>
/// Service for chat operations, including messages, conversations, reactions, and user search.
/// </summary>
public interface IChatService
{
    Task<ChatMessageDto> SendMessageAsync(SendMessageDto dto, string senderId);
    Task<ChatMessageDto> SendImageMessageAsync(SendImageMessageDto dto, string senderId, IFormFile image);
    Task<ChatMessageDto> SendVoiceMessageAsync(SendVoiceMessageDto dto, string senderId, IFormFile voice);

    /// <summary>
    /// Get paginated conversations for a user.
    /// </summary>
    Task<IEnumerable<ChatConversationDto>> GetUserConversationsAsync(string userId, int page = 1, int pageSize = 20);

    /// <summary>
    /// Get paginated messages for a conversation.
    /// </summary>
    Task<IEnumerable<ChatMessageDto>> GetConversationMessagesAsync(int conversationId, string userId, int page = 1, int pageSize = 30);

    Task<MessageReactionDto> ReactToMessageAsync(ReactToMessageDto dto, string userId);
    Task<(bool, DateTime?)> MarkMessageAsReadAsync(int messageId, string userId);
    Task<bool> DeleteMessageAsync(int messageId, string userId);
    Task<ChatMessageDto> EditMessageAsync(int messageId, string userId, string newText);

    /// <summary>
    /// Search users by name for starting new conversations.
    /// </summary>
    Task<IEnumerable<UserChatInfoDto>> SearchUsersAsync(string searchTerm, string currentUserId);

    /// <summary>
    /// Get count of unread messages for notification badge.
    /// </summary>
    Task<int> GetUnreadMessageCountAsync(string userId);

    /// <summary>
    /// Create a new conversation between two users.
    /// </summary>
    Task<ChatConversationDto> CreateConversationAsync(string user1Id, string user2Id);

    /// <summary>
    /// Delete a conversation.
    /// </summary>
    Task<bool> DeleteConversationAsync(int conversationId, string requestingUserId);


    Task<ChatConversationDto> GetConversationByTwoUsersIdsAsync(string user1Id, string user2Id);


    Task<ChatMessageDto> GetMessageByIdAsync(int messageId);
}