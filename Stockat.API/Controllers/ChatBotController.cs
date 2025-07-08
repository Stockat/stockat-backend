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
[AllowAnonymous]
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
        if (!string.IsNullOrEmpty(userId))
        {
            return userId;
        }
        // Anonymous user: use cookie or generate new
        var httpContext = HttpContext;
        var cookieName = "StockatChatAnonId";
        if (httpContext.Request.Cookies.TryGetValue(cookieName, out var anonId) && !string.IsNullOrEmpty(anonId))
        {
            return anonId;
        }
        // Generate new anon ID and set cookie
        var newAnonId = Guid.NewGuid().ToString();
        httpContext.Response.Cookies.Append(cookieName, newAnonId, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        });
        return newAnonId;
    }

    [HttpPost("ask")]
    [AllowAnonymous]
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

            // Save the user's message to chatbot history
            await _serviceManager.ChatHistoryService.SaveChatBotMessageAsync(userId, "user", message);
            _logger.LogInformation($"Saved user message to chatbot history");

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

            // Save the AI response to chatbot history
            await _serviceManager.ChatHistoryService.SaveChatBotMessageAsync(userId, "assistant", aiResponse);
            _logger.LogInformation($"Saved AI response to chatbot history");

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
    [AllowAnonymous]
    public async Task<IActionResult> GetChatHistory([FromQuery] int limit = 50)
    {
        try
        {
            if (limit <= 0 || limit > 100)
                return BadRequest(new { error = "Limit must be between 1 and 100." });

            var userId = GetUserId();
            _logger.LogInformation($"Retrieving chatbot history for user {userId}, limit: {limit}");
            
            var chatHistory = await _serviceManager.ChatHistoryService.GetChatBotHistoryAsync(userId, limit);

            _logger.LogInformation($"Retrieved chatbot history for user {userId}, found {chatHistory.Count()} messages");

            var response = new { 
                messages = chatHistory,
                userId = userId,
                totalMessages = chatHistory.Count()
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to chatbot history");
            return Unauthorized(new { error = "Authentication required to access chatbot history." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chatbot history");
            return StatusCode(500, new { error = "An error occurred while retrieving chatbot history." });
        }
    }

    [HttpDelete("history")]
    [AllowAnonymous]
    public async Task<IActionResult> ClearChatHistory()
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation($"Clearing chatbot history for user {userId}");
            
            await _serviceManager.ChatHistoryService.ClearChatBotHistoryAsync(userId);

            _logger.LogInformation($"Cleared chatbot history for user {userId}");

            return Ok(new { 
                message = "Chatbot history cleared successfully.",
                userId = userId
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to clear chatbot history");
            return Unauthorized(new { error = "Authentication required to clear chatbot history." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing chatbot history");
            return StatusCode(500, new { error = "An error occurred while clearing chatbot history." });
        }
    }
} 