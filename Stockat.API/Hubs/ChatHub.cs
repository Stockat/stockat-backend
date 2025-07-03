using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Stockat.Core.IServices;
using Stockat.Core.DTOs.ChatDTOs;
using Stockat.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Stockat.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IServiceManager _serviceManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChatHub(IServiceManager serviceManager, IHttpContextAccessor httpContextAccessor)
    {
        _serviceManager = serviceManager;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetUserId() =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.UserIdentifier;

    public async Task SendMessage(SendMessageDto dto)
    {
        if (dto == null || dto.ConversationId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid message data.");
            return;
        }
        var senderId = GetUserId();
        try
        {
            var message = await _serviceManager.ChatService.SendMessageAsync(dto, senderId);
            await Clients.Group($"conversation-{dto.ConversationId}").SendAsync("ReceiveMessage", message);
            // Notify recipient for unread counter (if not self-message)
            if (message.Sender.UserId != senderId)
                await Clients.User(message.Sender.UserId).SendAsync("IncrementUnread");
        }
        catch (System.Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task SendImageMessage(SendImageMessageDto dto, IFormFile image)
    {
        if (dto == null || dto.ConversationId <= 0 || image == null)
        {
            await Clients.Caller.SendAsync("Error", "Invalid image message data.");
            return;
        }
        var senderId = GetUserId();
        try
        {
            var message = await _serviceManager.ChatService.SendImageMessageAsync(dto, senderId, image);
            await Clients.Group($"conversation-{dto.ConversationId}").SendAsync("ReceiveMessage", message);
            if (message.Sender.UserId != senderId)
                await Clients.User(message.Sender.UserId).SendAsync("IncrementUnread");
        }
        catch (System.Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task SendVoiceMessage(SendVoiceMessageDto dto, IFormFile voice)
    {
        if (dto == null || dto.ConversationId <= 0 || voice == null)
        {
            await Clients.Caller.SendAsync("Error", "Invalid voice message data.");
            return;
        }
        var senderId = GetUserId();
        try
        {
            var message = await _serviceManager.ChatService.SendVoiceMessageAsync(dto, senderId, voice);
            await Clients.Group($"conversation-{dto.ConversationId}").SendAsync("ReceiveMessage", message);
            if (message.Sender.UserId != senderId)
                await Clients.User(message.Sender.UserId).SendAsync("IncrementUnread");
        }
        catch (System.Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task ReactToMessage(ReactToMessageDto dto)
    {
        if (dto == null || dto.MessageId <= 0 || string.IsNullOrWhiteSpace(dto.ReactionType))
        {
            await Clients.Caller.SendAsync("Error", "Invalid reaction data.");
            return;
        }

        var userId = GetUserId();
        await _serviceManager.ChatService.ReactToMessageAsync(dto, userId);

        var updatedMessage = await _serviceManager.ChatService.GetMessageByIdAsync(dto.MessageId);

        var groupName = $"conversation-{dto.ConversationId}";
        await Clients.Group(groupName).SendAsync("ReceiveMessageUpdate", updatedMessage);
    }
    //public async Task ReactToMessage(ReactToMessageDto dto)
    //{
    //    if (dto == null || dto.MessageId <= 0 || string.IsNullOrWhiteSpace(dto.ReactionType))
    //    {
    //        await Clients.Caller.SendAsync("Error", "Invalid reaction data.");
    //        return;
    //    }

    //    var userId = GetUserId();
    //    // This should add or remove the reaction and return the new state (null if removed)
    //    var reaction = await _serviceManager.ChatService.ReactToMessageAsync(dto, userId);

    //    // Use conversationId for the group name
    //    var groupName = $"conversation-{dto.ConversationId}";

    //    await Clients.Group(groupName).SendAsync("ReceiveReaction", new
    //    {
    //        MessageId = dto.MessageId,
    //        UserId = userId,
    //        ReactionType = dto.ReactionType,
    //        IsRemoved = reaction == null, // true if removed, false if added
    //        Reaction = reaction, // Should include userId, reactionType, etc.
    //        ConversationId = dto.ConversationId
    //    });
    //}


    public async Task MarkMessageAsRead(int messageId)
    {
        if (messageId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid message id.");
            return;
        }
        var message = await _serviceManager.ChatService.GetMessageByIdAsync(messageId);
        if (message == null) return;


        var userId = GetUserId();
        var (isRead, readAt) = await _serviceManager.ChatService.MarkMessageAsReadAsync(messageId, userId);
        //message = await _serviceManager.ChatService.GetMessageByIdAsync(messageId);
        if (isRead)
        {
            await Clients.Group($"conversation-{message.ConversationId}")
                     .SendAsync("MessageRead", messageId, userId, isRead, readAt);
        }
        else
        {
            await Clients.Group($"conversation-{message.ConversationId}")
                    .SendAsync("MessageRead", messageId, userId, isRead, null);
        }
        
        //await Clients.All.SendAsync("MessageRead", messageId, userId);
    }

    public async Task EditMessage(int messageId, string newText)
    {
        if (messageId <= 0 || string.IsNullOrWhiteSpace(newText))
        {
            await Clients.Caller.SendAsync("Error", "Invalid edit data.");
            return;
        }
        var userId = GetUserId();
        var message = await _serviceManager.ChatService.EditMessageAsync(messageId, userId, newText);
        if (message == null)
        {
            await Clients.Caller.SendAsync("Error", "Message not found or not allowed.");
            return;
        }
        await Clients.Group($"conversation-{message.ConversationId}").SendAsync("MessageEdited", message);
    }

    public async Task DeleteMessage(int messageId)
    {
        if (messageId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid message id.");
            return;
        }
        var userId = GetUserId();
        var result = await _serviceManager.ChatService.DeleteMessageAsync(messageId, userId);
        if (result)
            await Clients.All.SendAsync("MessageDeleted", messageId);
        else
            await Clients.Caller.SendAsync("Error", "Message not found or not allowed.");
    }

    public async Task JoinConversation(int conversationId)
    {
        if (conversationId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid conversation id.");
            return;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
    }

    public async Task LeaveConversation(int conversationId)
    {
        if (conversationId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid conversation id.");
            return;
        }
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
    }

    public async Task CreateConversation(string user2Id)
    {
        var user1Id = GetUserId();
        var conversation = await _serviceManager.ChatService.CreateConversationAsync(user1Id, user2Id);
        await Clients.User(user2Id).SendAsync("ConversationCreated", conversation);
        await Clients.Caller.SendAsync("ConversationCreated", conversation);
    }

    public async Task DeleteConversation(int conversationId)
    {
        if (conversationId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid conversation id.");
            return;
        }
        var userId = GetUserId();
        var result = await _serviceManager.ChatService.DeleteConversationAsync(conversationId, userId);
        if (result)
        {
            await Clients.Group($"conversation-{conversationId}").SendAsync("ConversationDeleted", conversationId);
        }
    }

    public async Task Typing(int conversationId)
    {
        var userId = GetUserId();
        await Clients.Group($"conversation-{conversationId}")
             .SendAsync("Typing", conversationId, userId);
    }

    public async Task Recording(int conversationId)
    {
        var userId = GetUserId();
        await Clients.Group($"conversation-{conversationId}")
             .SendAsync("Recording", conversationId, userId);
    }

}