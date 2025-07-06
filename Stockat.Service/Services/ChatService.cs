using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Stockat.Core;
using Stockat.Core.Consts;
using Stockat.Core.DTOs.ChatDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Entities.Chat;
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
    private readonly IConfiguration _configuration;
    public ChatService(
        IRepositoryManager repo,
        IMapper mapper,
        IImageService imageService,
        IFileService fileService,
        IConfiguration configuration
        )
    {
        _repo = repo;
        _mapper = mapper;
        _imageService = imageService;
        _fileService = fileService;
        _configuration = configuration;
    }

    private async Task<bool> IsUserDeletedAsync(string userId)
    {
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
        var sender = await _repo.UserRepo.GetByIdAsync(senderId);
        var message = new ChatMessage
        {
            ConversationId = dto.ConversationId,
            SenderId = senderId,
            MessageText = dto.MessageText,
            SentAt = DateTime.UtcNow,
            Sender = sender
        };
        await _repo.ChatMessageRepo.AddAsync(message);

        conversation.LastMessageAt = message.SentAt;
        _repo.ChatConversationRepo.Update(conversation);

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
        var sender = await _repo.UserRepo.GetByIdAsync(senderId);
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

        conversation.LastMessageAt = message.SentAt;
        _repo.ChatConversationRepo.Update(conversation);

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
        var sender = await _repo.UserRepo.GetByIdAsync(senderId);
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

        conversation.LastMessageAt = message.SentAt;
        _repo.ChatConversationRepo.Update(conversation);

        await _repo.CompleteAsync();
        return _mapper.Map<ChatMessageDto>(message);
    }

    public async Task<ChatConversationDto> CreateConversationAsync(string user1Id, string user2Id)
    {
        var existing = await _repo.ChatConversationRepo
            .FindAllAsync(c => (c.User1Id == user1Id && c.User2Id == user2Id) || (c.User1Id == user2Id && c.User2Id == user1Id));
        if (existing != null && existing.Any())
            throw new InvalidOperationException("Conversation already exists.");

        var user1 = await _repo.UserRepo.GetByIdAsync(user1Id);
        var user2 = await _repo.UserRepo.GetByIdAsync(user2Id);

        var conversation = new ChatConversation
        {
            User1Id = user1Id,
            User2Id = user2Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            User1 = user1,
            User2 = user2
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

        if (conversation.User1Id != requestingUserId && conversation.User2Id != requestingUserId)
            throw new UnauthorizedAccessException("Not a participant of this conversation.");

        // Optionally: delete related messages, reactions, read statuses, etc.
        _repo.ChatConversationRepo.Delete(conversation);
        await _repo.CompleteAsync();
        return true;
    }

    public async Task<IEnumerable<ChatConversationDto>> GetUserConversationsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var isAdmin = userId == _configuration["Admin:Id"];
        var adminId = _configuration["Admin:Id"];
        var conversations = new List<ChatConversation>();
        ChatConversation? adminConversation = null;

        if (!isAdmin && page == 1)
        {
            // Fetch more than pageSize in case admin conversation is among them
            var batch = await _repo.ChatConversationRepo.FindAllAsync(
                c => (c.User1Id == userId || c.User2Id == userId) && !c.User1.IsDeleted && !c.User2.IsDeleted,
                skip: 0,
                take: pageSize + 5, // Fetch extra in case admin is among them
                includes: new[] { "User1", "User2" },
                orderBy: c => c.LastMessageAt,
                orderByDirection: OrderBy.Descending
            );

            var batchList = batch.ToList();

            // Try find the admin conversation in the fetched batch
            var indexOfAdminConv = batchList.FindIndex(c =>
                (c.User1Id == userId && c.User2Id == adminId) ||
                (c.User2Id == userId && c.User1Id == adminId));

            if (indexOfAdminConv >= 0)
            {
                // Remove it from the list to prevent duplicates
                adminConversation = batchList[indexOfAdminConv];
                batchList.RemoveAt(indexOfAdminConv);
            }
            else
            {
                // If not in list, try fetch it directly
                adminConversation = await _repo.ChatConversationRepo.FindAsync(
                    c => (c.User1Id == userId && c.User2Id == adminId) ||
                         (c.User2Id == userId && c.User1Id == adminId),
                    includes: new[] { "User1", "User2" }
                );
            }

            // Take only the first pageSize items from the remaining batch
            conversations = batchList.Take(pageSize).ToList();

            // If we found admin conversation, insert it at top
            if (adminConversation != null)
            {
                conversations.Insert(0, adminConversation);
            }
        }
        else
        {
            // Regular paginated fetch for admin or pages beyond 1
            var skip = (page - 1) * pageSize;

            if (isAdmin)
            {

                var result = await _repo.ChatConversationRepo.FindAllAsync(
                    c => (c.User1Id == userId || c.User2Id == userId) &&
                         !c.User1.IsDeleted && !c.User2.IsDeleted,
                    skip: skip,
                    take: pageSize,
                    includes: new[] { "User1", "User2" },
                    orderBy: c => c.LastMessageAt,
                    orderByDirection: OrderBy.Descending

                );

                conversations = result.ToList();
            }
            else
            {
                var result = await _repo.ChatConversationRepo.FindAllAsync(
                    c => (c.User1Id == userId || c.User2Id == userId) &&
                         !c.User1.IsDeleted && !c.User2.IsDeleted &&
                         // Exclude admin conversation on all pages after page 1
                         (c.User1Id != adminId && c.User2Id != adminId),
                    skip: skip,
                    take: pageSize,
                    includes: new[] { "User1", "User2" },
                    orderBy: c => c.LastMessageAt,
                    orderByDirection: OrderBy.Descending
                );

                conversations = result.ToList();
            }
        }

        // Fetch last message for each conversation
        foreach (var conv in conversations)
        {
            var lastMsg = (await _repo.ChatMessageRepo.FindAllAsync(
                m => m.ConversationId == conv.ConversationId,
                skip: 0,
                take: 1,
                includes: new[] { "Sender", "Reactions", "ReadStatus" },
                orderBy: m => m.SentAt,
                orderByDirection: OrderBy.Descending
            )).FirstOrDefault();

            conv.Messages = lastMsg != null ? new List<ChatMessage> { lastMsg } : new List<ChatMessage>();
        }

        return _mapper.Map<IEnumerable<ChatConversationDto>>(conversations);
    }


    public async Task<IEnumerable<ChatMessageDto>> GetConversationMessagesAsync(int conversationId, string userId, int page = 1, int pageSize = 30)
    {
        var skip = (page - 1) * pageSize;
        var messages = await _repo.ChatMessageRepo.FindAllAsync(
            m => m.ConversationId == conversationId,
            skip: skip,
            take: pageSize,
            includes: new[] { "Sender", "Reactions", "ReadStatus" },
            orderBy: m => m.SentAt,
            orderByDirection: OrderBy.Descending
        );
        return _mapper.Map<IEnumerable<ChatMessageDto>>(messages);
    }

    public async Task<ChatConversationDto> GetConversationByTwoUsersIdsAsync(string user1Id, string user2Id)
    {
        var conversation = await _repo.ChatConversationRepo.FindAsync(
            c => (c.User1Id == user1Id && c.User2Id == user2Id) || (c.User1Id == user2Id && c.User2Id == user2Id), ["User1", "User2" ]
        );
        if (conversation is null) return null;
        return _mapper.Map<ChatConversationDto>(conversation);
    }

    public async Task<IEnumerable<UserChatInfoDto>> SearchUsersAsync(string searchTerm, string currentUserId)
    {
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

    //public async Task<MessageReactionDto?> ReactToMessageAsync(ReactToMessageDto dto, string userId)
    //{
    //    var existing = await _repo.MessageReactionRepo.FindAsync(
    //        r => r.MessageId == dto.MessageId &&
    //             r.UserId == userId &&
    //             r.ReactionType == dto.ReactionType);

    //    if (existing != null)
    //    {
    //        _repo.MessageReactionRepo.Delete(existing);
    //        await _repo.CompleteAsync();
    //        return null; // toggled off
    //    }

    //    var newReaction = new MessageReaction
    //    {
    //        MessageId = dto.MessageId,
    //        UserId = userId,
    //        ReactionType = dto.ReactionType,
    //        CreatedAt = DateTime.UtcNow
    //    };

    //    await _repo.MessageReactionRepo.AddAsync(newReaction);
    //    await _repo.CompleteAsync();

    //    return _mapper.Map<MessageReactionDto>(newReaction);
    //}

    public async Task<MessageReactionDto?> ReactToMessageAsync(ReactToMessageDto dto, string userId)
    {
        var existing = await _repo.MessageReactionRepo.FindAsync(
            r => r.MessageId == dto.MessageId &&
                 r.UserId == userId);

        if (existing != null)
        {
            if (existing.ReactionType == dto.ReactionType)
            {
                _repo.MessageReactionRepo.Delete(existing);
                await _repo.CompleteAsync();
                return null;
            }

            existing.ReactionType = dto.ReactionType;
            await _repo.CompleteAsync();
            return _mapper.Map<MessageReactionDto>(existing);
        }
        else
        {
            var newReaction = new MessageReaction
            {
                MessageId = dto.MessageId,
                UserId = userId,
                ReactionType = dto.ReactionType,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.MessageReactionRepo.AddAsync(newReaction);
            await _repo.CompleteAsync();
            return _mapper.Map<MessageReactionDto>(newReaction);
        }
    }




    public async Task<(bool, DateTime?)> MarkMessageAsReadAsync(int messageId, string userId)
    {
        var message = await _repo.ChatMessageRepo.FindAsync(m => m.MessageId == messageId, ["ReadStatus"]);
        if (message.IsRead) return (true, message.ReadStatus.ReadAt) ;

        if (message is null || message.SenderId == userId) return (false, null);

        var exists = await _repo.MessageReadStatusRepo.FindAsync(
            r => r.MessageId == messageId && r.UserId == userId);

        if (exists is not null)
            return (true, exists.ReadAt); // already marked as read before

        var readStatus = new MessageReadStatus
        {
            MessageId = messageId,
            UserId = userId,
            ReadAt = DateTime.UtcNow
        };

        message.IsRead = true;

        await _repo.MessageReadStatusRepo.AddAsync(readStatus);
        await _repo.CompleteAsync();
        return (true, DateTime.UtcNow);
    }

        //var readStatus = new MessageReadStatus
        //{
        //    MessageId = messageId,
        //    UserId = userId,
        //    ReadAt = DateTime.UtcNow
        //};
        //await _repo.MessageReadStatusRepo.AddAsync(readStatus);
        //await _repo.CompleteAsync();
        //return true;
    //}

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

    public async Task<ChatMessageDto> GetMessageByIdAsync(int messageId)
    {
        var message =  await _repo.ChatMessageRepo.FindAsync(cm => cm.MessageId == messageId, new[] { "Sender", "Reactions", "ReadStatus" });
        return _mapper.Map<ChatMessageDto>(message);
    }
}