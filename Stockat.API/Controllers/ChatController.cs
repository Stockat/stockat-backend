using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stockat.API.Hubs;
using Stockat.Core;
using Stockat.Core.DTOs.ChatDTOs;
using Stockat.Core.IServices;
using System.Security.Claims;

namespace Stockat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(IServiceManager serviceManager, IHubContext<ChatHub> hubContext)
    {
        _serviceManager = serviceManager;
        _hubContext = hubContext;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _serviceManager.ChatService.GetUserConversationsAsync(userId, page, pageSize);
        return Ok(result);
    }
    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<IActionResult> GetMessages(int conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
    {
        var userId = GetUserId();
        var result = await _serviceManager.ChatService.GetConversationMessagesAsync(conversationId, userId, page, pageSize);
        return Ok(result);
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDto dto)
    {
        var userId = GetUserId();
        if (userId == dto.User2Id)
            return BadRequest("Cannot create conversation with yourself.");

        var conversation = await _serviceManager.ChatService.CreateConversationAsync(userId, dto.User2Id);
        return Ok(conversation);
    }

    [HttpDelete("conversations/{conversationId}")]
    public async Task<IActionResult> DeleteConversation(int conversationId)
    {
        var userId = GetUserId();
        var result = await _serviceManager.ChatService.DeleteConversationAsync(conversationId, userId);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [HttpPost("messages/text")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        try
        {
            var result = await _serviceManager.ChatService.SendMessageAsync(dto, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("messages/image")]
    public async Task<IActionResult> SendImageMessage([FromForm] SendImageMessageDto dto)
    {
        if (!ModelState.IsValid || dto.Image == null)
            return BadRequest(ModelState);

        var userId = GetUserId();
        try
        {
            var result = await _serviceManager.ChatService.SendImageMessageAsync(dto, userId, dto.Image);
            await _hubContext.Clients.Group($"conversation-{dto.ConversationId}").SendAsync("ReceiveMessage", result);
            if (result.Sender.UserId != GetUserId())
                await _hubContext.Clients.User(result.Sender.UserId).SendAsync("IncrementUnread");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("messages/voice")]
    public async Task<IActionResult> SendVoiceMessage([FromForm] SendVoiceMessageDto dto)
    {
        if (!ModelState.IsValid || dto.Voice == null)
            return BadRequest(ModelState);

        var userId = GetUserId();
        try
        {
            var result = await _serviceManager.ChatService.SendVoiceMessageAsync(dto, userId, dto.Voice);
            await _hubContext.Clients.Group($"conversation-{dto.ConversationId}").SendAsync("ReceiveMessage", result);
            if (result.Sender.UserId != GetUserId())
                await _hubContext.Clients.User(result.Sender.UserId).SendAsync("IncrementUnread");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpPost("messages/react")]
    public async Task<IActionResult> ReactToMessage([FromBody] ReactToMessageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);


        var userId = GetUserId();

        var reaction = await _serviceManager.ChatService.ReactToMessageAsync(dto, userId);

        var groupName = $"conversation-{dto.ConversationId}";

        var result = new
        {
            MessageId = dto.MessageId,
            UserId = userId,
            ReactionType = dto.ReactionType,
            IsRemoved = reaction == null, // true if removed, false if added
            Reaction = reaction, // Should include userId, reactionType, etc.
            ConversationId = dto.ConversationId
        };
        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveReaction", result);
        return Ok(result);
    }

    [HttpPost("messages/{messageId}/read")]
    public async Task<IActionResult> MarkMessageAsRead(int messageId)
    {
        var userId = GetUserId();
        await _serviceManager.ChatService.MarkMessageAsReadAsync(messageId, userId);
        return Ok();
    }

    [HttpPut("messages/{messageId}")]
    public async Task<IActionResult> EditMessage(int messageId, [FromBody] string newText)
    {
        if (string.IsNullOrWhiteSpace(newText))
            return BadRequest("Message text cannot be empty.");

        var userId = GetUserId();
        var result = await _serviceManager.ChatService.EditMessageAsync(messageId, userId, newText);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpDelete("messages/{messageId}")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        var userId = GetUserId();
        var result = await _serviceManager.ChatService.DeleteMessageAsync(messageId, userId);
        if (!result)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("users/search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string term)
    {
        var userId = GetUserId();
        var result = await _serviceManager.ChatService.SearchUsersAsync(term, userId);
        return Ok(result);
    }

    [HttpGet("notifications/unread-count")]
    public async Task<IActionResult> GetUnreadMessageCount()
    {
        var userId = GetUserId();
        var count = await _serviceManager.ChatService.GetUnreadMessageCountAsync(userId);
        return Ok(count);
    }
}