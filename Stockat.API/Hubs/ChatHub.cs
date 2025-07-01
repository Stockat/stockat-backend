using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Stockat.Core.IServices;
using Stockat.Core.DTOs.ChatDTOs;
using Stockat.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

/// <summary>
/// SignalR hub for real-time chat events.
/// All DateTime values are UTC. The frontend should convert to the user's local time zone.
/// </summary>
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

    /// <summary>
    /// Send a text message to a conversation group.
    /// Allows self-messaging (user can send messages to themselves).
    /// </summary>
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

    /// <summary>
    /// Send an image message to a conversation group.
    /// </summary>
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
            await Clients.User(message.Sender.UserId).SendAsync("IncrementUnread");
        }
        catch (System.Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    /// <summary>
    /// Send a voice message to a conversation group.
    /// </summary>
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
            await Clients.User(message.Sender.UserId).SendAsync("IncrementUnread");
        }
        catch (System.Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    /// <summary>
    /// React to a message in a conversation.
    /// </summary>
    public async Task ReactToMessage(ReactToMessageDto dto)
    {
        if (dto == null || dto.MessageId <= 0 || string.IsNullOrWhiteSpace(dto.ReactionType))
        {
            await Clients.Caller.SendAsync("Error", "Invalid reaction data.");
            return;
        }
        var userId = GetUserId();
        var reaction = await _serviceManager.ChatService.ReactToMessageAsync(dto, userId);
        await Clients.Group($"conversation-{reaction.MessageId}").SendAsync("ReceiveReaction", reaction);
    }

    /// <summary>
    /// Mark a message as read.
    /// </summary>
    public async Task MarkMessageAsRead(int messageId)
    {
        if (messageId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid message id.");
            return;
        }
        var userId = GetUserId();
        await _serviceManager.ChatService.MarkMessageAsReadAsync(messageId, userId);
        await Clients.All.SendAsync("MessageRead", messageId, userId);
    }

    /// <summary>
    /// Edit a message.
    /// </summary>
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

    /// <summary>
    /// Delete a message.
    /// </summary>
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

    /// <summary>
    /// Join a conversation group for real-time updates.
    /// </summary>
    public async Task JoinConversation(int conversationId)
    {
        if (conversationId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid conversation id.");
            return;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
    }

    /// <summary>
    /// Leave a conversation group.
    /// </summary>
    public async Task LeaveConversation(int conversationId)
    {
        if (conversationId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid conversation id.");
            return;
        }
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
    }

    /// <summary>
    /// Create a new conversation.
    /// </summary>
    public async Task CreateConversation(string user2Id)
    {
        var user1Id = GetUserId();
        var conversation = await _serviceManager.ChatService.CreateConversationAsync(user1Id, user2Id);
        await Clients.User(user2Id).SendAsync("ConversationCreated", conversation);
        await Clients.Caller.SendAsync("ConversationCreated", conversation);
    }

    /// <summary>
    /// Delete a conversation.
    /// </summary>
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
}