using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core;
using Stockat.Core.DTOs.ChatDTOs;
using Stockat.Core.IServices;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

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
        // First try to get user ID from the authenticated user context
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            _logger.LogInformation($"GetUserId: Found authenticated user ID: {userId}");
            return userId;
        }

        // If no authenticated user, try to manually validate the JWT token from the Authorization header
        var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            try
            {
                var token = authHeader.Substring("Bearer ".Length);
                var jwtSettings = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetSection("JwtSettings");
                var secretKey = Environment.GetEnvironmentVariable("JWTSECRET");
                
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);
                
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["validIssuer"],
                    ValidAudience = jwtSettings["validAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                
                userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    _logger.LogInformation($"GetUserId: Found user ID from JWT token: {userId}");
                    return userId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"GetUserId: Failed to validate JWT token: {ex.Message}");
            }
        }

        // Anonymous user: use cookie or generate new
        var httpContext = HttpContext;
        var cookieName = "StockatChatAnonId";
        if (httpContext.Request.Cookies.TryGetValue(cookieName, out var anonId) && !string.IsNullOrEmpty(anonId))
        {
            _logger.LogInformation($"GetUserId: Found anonymous user ID from cookie: {anonId}");
            return anonId;
        }
        // Generate new anon ID and set cookie
        var newAnonId = Guid.NewGuid().ToString();
        _logger.LogInformation($"GetUserId: Generated new anonymous user ID: {newAnonId}");
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

            // Get chat history for context
            var chatHistory = await _serviceManager.ChatHistoryService.GetChatBotHistoryAsync(userId, 10);

            // Fetch platform data for context
            var platformData = await GetPlatformDataForContext();
            
            // Generate AI response with context
            var contextData = new
            {
                userId = userId,
                timestamp = DateTime.UtcNow,
                platform = "Stockat B2B Manufacturing Platform",
                userEmail = User.FindFirstValue(ClaimTypes.Email),
                userRole = User.FindFirstValue(ClaimTypes.Role),
                platformData = platformData
            };

            string aiResponse;
            
            // Check if this is a specific query that should use our AIService directly
            var messageLower = message.ToLower().Trim();
            var useDirectAIService = messageLower.Contains("top seller") || 
                                   messageLower.Contains("best seller") || 
                                   messageLower.Contains("popular product") || 
                                   messageLower.Contains("best product") || 
                                   messageLower.Contains("live auction") || 
                                   messageLower.Contains("auction") || 
                                   messageLower.Contains("service") || 
                                   messageLower.Contains("category") || 
                                   messageLower.Contains("statistics") || 
                                   messageLower.Contains("stats");
            
            if (useDirectAIService)
            {
                _logger.LogInformation($"ChatBot: Using direct AIService for specific query: {message}");
                aiResponse = await _serviceManager.AIService.GenerateResponseAsync(message, contextData);
                _logger.LogInformation($"ChatBot: Direct AIService response generated, length: {aiResponse?.Length ?? 0}");
            }
            else
            {
                try
                {
                    _logger.LogInformation($"ChatBot: Attempting OpenAI call for user {userId}");
                    
                    // Try OpenAI without chat history first
                    aiResponse = await _serviceManager.OpenAIService.GenerateResponseAsync(message, contextData);
                    _logger.LogInformation($"ChatBot: OpenAI call successful, response length: {aiResponse?.Length ?? 0}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"OpenAI failed, falling back to platform data: {ex.Message}");
                    _logger.LogWarning($"OpenAI exception details: {ex}");
                    
                    // Try with chat history as second attempt
                    try
                    {
                        aiResponse = await _serviceManager.OpenAIService.GenerateResponseWithHistoryAsync(message, chatHistory, contextData);
                        _logger.LogInformation($"ChatBot: OpenAI with history successful, response length: {aiResponse?.Length ?? 0}");
                    }
                    catch (Exception ex2)
                    {
                        _logger.LogWarning($"OpenAI with history also failed: {ex2.Message}");
                        // Fallback to regular AI service
                        aiResponse = await _serviceManager.AIService.GenerateResponseAsync(message, contextData);
                        _logger.LogInformation($"ChatBot: Fallback response generated, length: {aiResponse?.Length ?? 0}");
                    }
                }
            }

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
            _logger.LogInformation($"Chat history details: {string.Join(", ", chatHistory.Select(m => $"Role: {m.Role}, Content: {m.MessageText?.Substring(0, Math.Min(50, m.MessageText?.Length ?? 0))}"))}");

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

    [HttpGet("test")]
    [AllowAnonymous]
    public async Task<IActionResult> TestChatBot()
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation($"Test endpoint called for user {userId}");
            
            // Check if there are any messages in the database
            var allMessages = await _serviceManager.ChatHistoryService.GetChatBotHistoryAsync(userId, 100);
            _logger.LogInformation($"Found {allMessages.Count()} messages for user {userId}");
            
            return Ok(new { 
                userId = userId,
                messageCount = allMessages.Count(),
                messages = allMessages.Take(5).ToList() // Return first 5 messages for debugging
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test endpoint");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private async Task<object> GetPlatformDataForContext()
    {
        try
        {
            // Fetch relevant platform data based on common chatbot queries
            var topSellers = await _serviceManager.AnalyticsService.GetTopSellersAsync(5);
            var topProducts = await _serviceManager.AnalyticsService.GetTopSellingProductsAsync(5);
            var liveAuctions = await _serviceManager.AnalyticsService.GetLiveAuctionsAsync();
            var topServices = await _serviceManager.AnalyticsService.GetTopUsedServicesAsync(5);
            var categoryStats = await _serviceManager.AnalyticsService.GetCategoryStatsAsync();

            // --- Fetch all approved/not deleted sellers ---
            var allSellersResult = await _serviceManager.UserService.GetAllUsersAsync(1, 10000, null, true, true, false); // isActive: true, isVerified: true, isBlocked: false
            var allSellers = allSellersResult.Data.PaginatedData
                .Where(u => u.Roles != null && u.Roles.Contains("Seller"))
                .Select(s => new { s.Id, s.FirstName, s.LastName, s.Email });

            // --- Fetch all approved/not deleted services ---
            var allServicesResult = await _serviceManager.ServiceService.GetAllServicesForAdminAsync(1, 10000, false, false, false);
            var allServices = allServicesResult.Data.PaginatedData
                .Where(s => s.IsApproved == Stockat.Core.Enums.ApprovalStatus.Approved && !s.IsDeleted)
                .Select(s => new { s.Id, s.Name, s.Description, s.SellerId });

            // --- Fetch all approved/not deleted products ---
            var allProductsResult = await _serviceManager.ProductService.getAllProductsPaginatedForAdmin(10000, 1, null, 0, 0, 0, new int[0], false, "Approved");
            var allProducts = allProductsResult.Data.PaginatedData
                .Select(p => new { p.Id, p.Name, p.Description, p.SellerId, p.CategoryName });

            // --- Fetch all auctions (not deleted) ---
            var allAuctions = (await _serviceManager.AuctionService.GetAllAuctionsAsync())
                .Where(a => a.IsDeleted == false)
                .Select(a => new { a.Id, a.Name, a.ProductId, a.SellerId, a.EndTime });

            _logger.LogInformation($"GetPlatformDataForContext: Found {topSellers.Count()} top sellers, {topProducts.Count()} top products, {liveAuctions.Count()} live auctions, {topServices.Count()} top services, {categoryStats.Count()} categories");

            var result = new
            {
                topSellers = topSellers.Select(s => new { s.Email, s.FirstName, s.LastName, s.AboutMe, s.City, s.Country, s.IsApproved, s.PhoneNumber }),
                topProducts = topProducts.Select(p => new { p.Name, p.Description, p.Price, p.CategoryName }),
                liveAuctions = liveAuctions.Select(a => new { a.Name, a.EndTime, a.CurrentBid, a.ProductName }),
                topServices = topServices.Select(s => new { s.Name, s.Description, s.PricePerProduct, s.SellerName }),
                categories = categoryStats.Select(c => new { c.CategoryName }),
                platformStats = new
                {
                    totalSellers = topSellers.Count(),
                    totalProducts = topProducts.Count(),
                    liveAuctionsCount = liveAuctions.Count(),
                    totalServices = topServices.Count(),
                    totalCategories = categoryStats.Count()
                },
                // --- Add summary lists for context awareness ---
                allSellers = allSellers,
                allServices = allServices,
                allProducts = allProducts,
                allAuctions = allAuctions
            };

            _logger.LogInformation($"GetPlatformDataForContext: Returning platform data with {result.topSellers.Count()} sellers, {result.topProducts.Count()} products, {result.liveAuctions.Count()} auctions, {result.topServices.Count()} services, {result.categories.Count()} categories, {allSellers.Count()} all sellers, {allServices.Count()} all services, {allProducts.Count()} all products, {allAuctions.Count()} all auctions");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error fetching platform data for chatbot context: {ex.Message}");
            return new { error = "Unable to fetch platform data" };
        }
    }
} 