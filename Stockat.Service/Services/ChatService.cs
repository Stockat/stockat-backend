using AutoMapper;
using Microsoft.AspNetCore.Http;
using Stockat.Core;
using Stockat.Core.DTOs.ChatDTOs;
using Stockat.Core.Entities.Chat;
using Stockat.Core.Entities;
using Stockat.Core.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Stockat.Service.Services;

public class ChatService : IChatService
{
    private readonly IRepositoryManager _repo;
    private readonly IMapper _mapper;
    private readonly IImageService _imageService;
    private readonly IFileService _fileService;

    public ChatService(
        IRepositoryManager repo,
        IMapper mapper,
        IImageService imageService,
        IFileService fileService)
    {
        _repo = repo;
        _mapper = mapper;
        _imageService = imageService;
        _fileService = fileService;
    }

    // Helper: Check if user is deleted
    private async Task<bool> IsUserDeletedAsync(string userId)
    {
        // Use string overload for User entity
        var user = await _repo.UserRepo.GetByIdAsync(userId);
        return user?.IsDeleted ?? true;
    }

    public async Task<ChatMessageDto> SendMessageAsync(SendMessageDto dto, string senderId)
    {
        var conversation = await _repo.ChatConversationRepo.GetByIdAsync(dto.ConversationId);
        if (conversation == null)
            throw new Exception("Conversation not found.");

        var recipientId = (conversation.User1Id == senderId && conversation.User2Id == senderId)
            ? senderId
            : (conversation.User1Id == senderId ? conversation.User2Id : conversation.User1Id);

        if (await IsUserDeletedAsync(recipientId))
            throw new Exception("Cannot send message to a deleted user.");

        var message = new ChatMessage
        {
            ConversationId = dto.ConversationId,
            SenderId = senderId,
            MessageText = dto.MessageText,
            SentAt = DateTime.UtcNow
        };
        await _repo.ChatMessageRepo.AddAsync(message);
        await _repo.CompleteAsync();
        return _mapper.Map<ChatMessageDto>(message);
    }

    public async Task<ChatMessageDto> SendImageMessageAsync(SendImageMessageDto dto, string senderId, IFormFile image)
    {
        var conversation = await _repo.ChatConversationRepo.GetByIdAsync(dto.ConversationId);
        if (conversation == null)
            throw new Exception("Conversation not found.");

        var recipientId = (conversation.User1Id == senderId && conversation.User2Id == senderId)
            ? senderId
            : (conversation.User1Id == senderId ? conversation.User2Id : conversation.User1Id);

        if (await IsUserDeletedAsync(recipientId))
            throw new Exception("Cannot send message to a deleted user.");

        var uploadResult = await _imageService.UploadImageAsync(image, "/ChatImages");
        var message = new ChatMessage
        {
            ConversationId = dto.ConversationId,
            SenderId = senderId,
            ImageUrl = uploadResult.Url,
            ImageId = uploadResult.FileId,
            MessageText = dto.MessageText,
            SentAt = DateTime.UtcNow
        };
        await _repo.ChatMessageRepo.AddAsync(message);
        await _repo.CompleteAsync();
        return _mapper.Map<ChatMessageDto>(message);
    }

    public async Task<ChatMessageDto> SendVoiceMessageAsync(SendVoiceMessageDto dto, string senderId, IFormFile voice)
    {
        var conversation = await _repo.ChatConversationRepo.GetByIdAsync(dto.ConversationId);
        if (conversation == null)
            throw new Exception("Conversation not found.");

        var recipientId = (conversation.User1Id == senderId && conversation.User2Id == senderId)
            ? senderId
            : (conversation.User1Id == senderId ? conversation.User2Id : conversation.User1Id);

        if (await IsUserDeletedAsync(recipientId))
            throw new Exception("Cannot send message to a deleted user.");

        var uploadResult = await _fileService.UploadFileAsync(voice);
        var message = new ChatMessage
        {
            ConversationId = dto.ConversationId,
            SenderId = senderId,
            VoiceUrl = uploadResult.Url,
            VoiceId = uploadResult.PublicId,
            MessageText = dto.MessageText,
            SentAt = DateTime.UtcNow
        };
        await _repo.ChatMessageRepo.AddAsync(message);
        await _repo.CompleteAsync();
        return _mapper.Map<ChatMessageDto>(message);
    }

    public async Task<ChatConversationDto> CreateConversationAsync(string user1Id, string user2Id)
    {
        // Prevent duplicate conversations
        var existing = await _repo.ChatConversationRepo
            .FindAsync(c => (c.User1Id == user1Id && c.User2Id == user2Id) || (c.User1Id == user2Id && c.User2Id == user1Id));
        if (existing.Any())
            throw new InvalidOperationException("Conversation already exists.");

        var conversation = new ChatConversation
        {
            User1Id = user1Id,
            User2Id = user2Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        await _repo.ChatConversationRepo.AddAsync(conversation);
        await _repo.CompleteAsync();

        return _mapper.Map<ChatConversationDto>(conversation);
    }

    public async Task<bool> DeleteConversationAsync(int conversationId, string requestingUserId)
    {
        var conversation = await _repo.ChatConversationRepo.GetByIdAsync(conversationId);
        if (conversation == null)
            return false;

        // Only allow participants to delete
        if (conversation.User1Id != requestingUserId && conversation.User2Id != requestingUserId)
            throw new UnauthorizedAccessException("Not a participant of this conversation.");

        // Optionally: delete related messages, reactions, read statuses, etc.
        _repo.ChatConversationRepo.Delete(conversation);
        await _repo.CompleteAsync();
        return true;
    }

    /// <summary>
    /// Get paginated conversations for a user, including the last message for each conversation.
    /// All DateTime values are UTC; frontend should convert to local time zone.
    /// </summary>
    public async Task<IEnumerable<ChatConversationDto>> GetUserConversationsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var skip = (page - 1) * pageSize;
        // Use FindAllAsync for correct skip/take/orderBy/includes
        var conversations = await _repo.ChatConversationRepo.FindAllAsync(
            c => (c.User1Id == userId || c.User2Id == userId) && !c.User1.IsDeleted && !c.User2.IsDeleted,
            skip: skip,
            take: pageSize,
            includes: new[] { "User1", "User2" },
            orderBy: c => c.LastMessageAt,
            orderByDirection: "Descending"
        );

        var conversationList = conversations.ToList();

        // For each conversation, fetch the last message (if any) with sender info
        foreach (var conv in conversationList)
        {
            var lastMsg = (await _repo.ChatMessageRepo.FindAllAsync(
                m => m.ConversationId == conv.ConversationId,
                skip: 0,
                take: 1,
                includes: new[] { "Sender", "Reactions", "ReadStatuses" },
                orderBy: m => m.SentAt,
                orderByDirection: "Descending"
            )).FirstOrDefault();

            if (lastMsg != null)
            {
                conv.Messages = new List<ChatMessage> { lastMsg };
            }
            else
            {
                conv.Messages = new List<ChatMessage>();
            }
        }

        return _mapper.Map<IEnumerable<ChatConversationDto>>(conversationList);
    }

    /// <summary>
    /// Get paginated messages for a conversation.
    /// All DateTime values are UTC; frontend should convert to local time zone.
    /// </summary>
    public async Task<IEnumerable<ChatMessageDto>> GetConversationMessagesAsync(int conversationId, string userId, int page = 1, int pageSize = 30)
    {
        var skip = (page - 1) * pageSize;
        var messages = await _repo.ChatMessageRepo.FindAllAsync(
            m => m.ConversationId == conversationId,
            skip: skip,
            take: pageSize,
            includes: new[] { "Sender", "Reactions", "ReadStatuses" },
            orderBy: m => m.SentAt,
            orderByDirection: "Descending"
        );
        // Return in ascending order for chat display
        return _mapper.Map<IEnumerable<ChatMessageDto>>(messages.OrderBy(m => m.SentAt));
    }

    public async Task<IEnumerable<UserChatInfoDto>> SearchUsersAsync(string searchTerm, string currentUserId)
    {
        // Use FindAllAsync for filtering
        var users = await _repo.UserRepo.FindAllAsync(
            u => (u.FirstName + " " + u.LastName).Contains(searchTerm) && u.Id != currentUserId && !u.IsDeleted
        );
        return _mapper.Map<IEnumerable<UserChatInfoDto>>(users);
    }

    public async Task<int> GetUnreadMessageCountAsync(string userId)
    {
        var count = await _repo.ChatMessageRepo
            .CountAsync(m => (m.Conversation.User1Id == userId || m.Conversation.User2Id == userId)
                && !m.IsRead && m.SenderId != userId);
        return count;
    }

    public async Task<MessageReactionDto> ReactToMessageAsync(ReactToMessageDto dto, string userId)
    {
        var reaction = new MessageReaction
        {
            MessageId = dto.MessageId,
            UserId = userId,
            ReactionType = dto.ReactionType,
            CreatedAt = DateTime.UtcNow
        };
        await _repo.MessageReactionRepo.AddAsync(reaction);
        await _repo.CompleteAsync();
        return _mapper.Map<MessageReactionDto>(reaction);
    }

    public async Task<bool> MarkMessageAsReadAsync(int messageId, string userId)
    {
        var readStatus = new MessageReadStatus
        {
            MessageId = messageId,
            UserId = userId,
            ReadAt = DateTime.UtcNow
        };
        await _repo.MessageReadStatusRepo.AddAsync(readStatus);
        await _repo.CompleteAsync();
        return true;
    }

    public async Task<bool> DeleteMessageAsync(int messageId, string userId)
    {
        var message = await _repo.ChatMessageRepo.GetByIdAsync(messageId);
        if (message == null || message.SenderId != userId)
            return false;
        _repo.ChatMessageRepo.Delete(message);
        await _repo.CompleteAsync();
        return true;
    }

    public async Task<ChatMessageDto> EditMessageAsync(int messageId, string userId, string newText)
    {
        var message = await _repo.ChatMessageRepo.GetByIdAsync(messageId);
        if (message == null || message.SenderId != userId)
            return null;
        message.MessageText = newText;
        message.IsEdited = true;
        _repo.ChatMessageRepo.Update(message);
        await _repo.CompleteAsync();
        return _mapper.Map<ChatMessageDto>(message);
    }
}