using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core.DTOs.ChatDTOs;
using Stockat.Core.IServices;
using Stockat.Core;
using System.Security.Claims;

namespace Stockat.API.Controllers;

/// <summary>
/// Controller for chat operations (REST endpoints).
/// All DateTime values are returned as UTC. The frontend should convert to the user's local time zone.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public ChatController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Get paginated conversations for the current user.
    /// Each conversation includes the last message for preview.
    /// All DateTime values are UTC.
    /// </summary>
    /// <param name="page">Page number (default 1)</param>
    /// <param name="pageSize">Page size (default 20)</param>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _serviceManager.ChatService.GetUserConversationsAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get paginated messages for a conversation.
    /// </summary>
    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<IActionResult> GetMessages(int conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
    {
        var userId = GetUserId();
        var result = await _serviceManager.ChatService.GetConversationMessagesAsync(conversationId, userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Create a new conversation.
    /// </summary>
    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDto dto)
    {
        var userId = GetUserId();
        if (userId == dto.User2Id)
            return BadRequest("Cannot create conversation with yourself.");

        var conversation = await _serviceManager.ChatService.CreateConversationAsync(userId, dto.User2Id);
        return Ok(conversation);
    }

    /// <summary>
    /// Delete a conversation.
    /// </summary>
    [HttpDelete("conversations/{conversationId}")]
    public async Task<IActionResult> DeleteConversation(int conversationId)
    {
        var userId = GetUserId();
        var result = await _serviceManager.ChatService.DeleteConversationAsync(conversationId, userId);
        if (!result)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Send a text message.
    /// </summary>
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

    /// <summary>
    /// Send an image message.
    /// </summary>
    [HttpPost("messages/image")]
    public async Task<IActionResult> SendImageMessage([FromForm] SendImageMessageDto dto)
    {
        if (!ModelState.IsValid || dto.Image == null)
            return BadRequest(ModelState);

        var userId = GetUserId();
        try
        {
            var result = await _serviceManager.ChatService.SendImageMessageAsync(dto, userId, dto.Image);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Send a voice message.
    /// </summary>
    [HttpPost("messages/voice")]
    public async Task<IActionResult> SendVoiceMessage([FromForm] SendVoiceMessageDto dto)
    {
        if (!ModelState.IsValid || dto.Voice == null)
            return BadRequest(ModelState);

        var userId = GetUserId();
        try
        {
            var result = await _serviceManager.ChatService.SendVoiceMessageAsync(dto, userId, dto.Voice);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    /// <summary>
    /// React to a message.
    /// </summary>
    [HttpPost("messages/react")]
    public async Task<IActionResult> ReactToMessage([FromBody] ReactToMessageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        var result = await _serviceManager.ChatService.ReactToMessageAsync(dto, userId);
        return Ok(result);
    }

    /// <summary>
    /// Mark a message as read.
    /// </summary>
    [HttpPost("messages/{messageId}/read")]
    public async Task<IActionResult> MarkMessageAsRead(int messageId)
    {
        var userId = GetUserId();
        await _serviceManager.ChatService.MarkMessageAsReadAsync(messageId, userId);
        return Ok();
    }

    /// <summary>
    /// Edit a message.
    /// </summary>
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

    /// <summary>
    /// Delete a message.
    /// </summary>
    [HttpDelete("messages/{messageId}")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        var userId = GetUserId();
        var result = await _serviceManager.ChatService.DeleteMessageAsync(messageId, userId);
        if (!result)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Search users by name for starting new conversations.
    /// </summary>
    [HttpGet("users/search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string term)
    {
        var userId = GetUserId();
        var result = await _serviceManager.ChatService.SearchUsersAsync(term, userId);
        return Ok(result);
    }

    /// <summary>
    /// Get count of unread messages for notification badge.
    /// </summary>
    [HttpGet("notifications/unread-count")]
    public async Task<IActionResult> GetUnreadMessageCount()
    {
        var userId = GetUserId();
        var count = await _serviceManager.ChatService.GetUnreadMessageCountAsync(userId);
        return Ok(count);
    }
}