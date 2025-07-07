using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core;
using Stockat.Core.DTOs.ChatDTOs;
using Stockat.Core.IServices;
using System.Security.Claims;
using System.Text.Json;

namespace Stockat.API.Controllers;

[ApiController]
[Route("api/chatbot")]
public class ChatBotController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly ILogger<ChatBotController> _logger;

    public ChatBotController(
        IServiceManager serviceManager,
        ILogger<ChatBotController> logger)
    {
        _serviceManager = serviceManager;
        _logger = logger;
    }

    private string GetUserId() 
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User must be authenticated to use the chatbot.");
        }
        return userId;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> AskChatBot([FromBody] ChatRequestDto request)
    {
        try
        {
            var message = request.Message?.Trim();

            if (string.IsNullOrWhiteSpace(message))
                return BadRequest(new { error = "Message cannot be empty." });

            if (message.Length > 1000)
                return BadRequest(new { error = "Message is too long. Maximum 1000 characters allowed." });

            var userId = GetUserId();
            _logger.LogInformation($"Processing chatbot request for user {userId}: {message}");

            // Save the user's message to chat history
            await _serviceManager.ChatHistoryService.SaveMessageAsync(userId, "user", message);
            _logger.LogInformation($"Saved user message to history");

            // Generate AI response
            var contextData = new
            {
                userId = userId,
                timestamp = DateTime.UtcNow,
                platform = "Stockat B2B Manufacturing Platform",
                userEmail = User.FindFirstValue(ClaimTypes.Email),
                userRole = User.FindFirstValue(ClaimTypes.Role)
            };

            var aiResponse = await _serviceManager.AIService.GenerateResponseAsync(message, contextData);
            _logger.LogInformation($"Generated AI response: {aiResponse}");

            // Save the AI response to chat history
            await _serviceManager.ChatHistoryService.SaveMessageAsync(userId, "assistant", aiResponse);
            _logger.LogInformation($"Saved AI response to history");

            var response = new { 
                response = aiResponse,
                timestamp = DateTime.UtcNow,
                userId = userId
            };

            _logger.LogInformation($"Returning response: {JsonSerializer.Serialize(response)}");
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to chatbot");
            return Unauthorized(new { error = "Authentication required to use the chatbot." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chatbot request");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetChatHistory([FromQuery] int limit = 50)
    {
        try
        {
            if (limit <= 0 || limit > 100)
                return BadRequest(new { error = "Limit must be between 1 and 100." });

            var userId = GetUserId();
            _logger.LogInformation($"Retrieving chat history for user {userId}, limit: {limit}");
            
            var chatHistory = await _serviceManager.ChatHistoryService.GetChatHistoryAsync(userId, limit);

            _logger.LogInformation($"Retrieved chat history for user {userId}, found {chatHistory.Count()} messages");

            var response = new { 
                messages = chatHistory,
                userId = userId,
                totalMessages = chatHistory.Count()
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to chat history");
            return Unauthorized(new { error = "Authentication required to access chat history." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat history");
            return StatusCode(500, new { error = "An error occurred while retrieving chat history." });
        }
    }

    [HttpDelete("history")]
    public async Task<IActionResult> ClearChatHistory()
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation($"Clearing chat history for user {userId}");
            
            await _serviceManager.ChatHistoryService.ClearChatHistoryAsync(userId);

            _logger.LogInformation($"Cleared chat history for user {userId}");

            return Ok(new { 
                message = "Chat history cleared successfully.",
                userId = userId
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to clear chat history");
            return Unauthorized(new { error = "Authentication required to clear chat history." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing chat history");
            return StatusCode(500, new { error = "An error occurred while clearing chat history." });
        }
    }
} 