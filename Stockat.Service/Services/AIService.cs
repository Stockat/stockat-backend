using Stockat.Core;
using Stockat.Core.IServices;
using System.Text.Json;

namespace Stockat.Service.Services;

public class AIService : IAIService
{
    private readonly IServiceManager _serviceManager;
    private readonly ILoggerManager _logger;

    public AIService(IServiceManager serviceManager, ILoggerManager logger)
    {
        _serviceManager = serviceManager;
        _logger = logger;
    }

    public async Task<string> GenerateResponseAsync(string userMessage, object contextData)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return "I didn't receive a message. Please try again.";
            }

            var message = userMessage.ToLower().Trim();
            
            // Get real platform data to include in context
            var platformData = await GetPlatformDataForContext();
            
            // Get real data from platform services
            if (message.Contains("top seller") || message.Contains("best seller") || message.Contains("seller"))
            {
                return await GetTopSellersInfo();
            }
            
            if (message.Contains("popular product") || message.Contains("best product") || message.Contains("product"))
            {
                return await GetPopularProductsInfo();
            }
            
            if (message.Contains("auction") || message.Contains("live auction") || message.Contains("bid"))
            {
                return await GetLiveAuctionsInfo();
            }
            
            if (message.Contains("service") || message.Contains("popular service"))
            {
                return await GetPopularServicesInfo();
            }
            
            if (message.Contains("category") || message.Contains("product category"))
            {
                return await GetCategoriesInfo();
            }
            
            if (message.Contains("help") || message.Contains("what can you do") || message.Contains("assist"))
            {
                return await GetHelpInfo();
            }
            
            if (message.Contains("statistics") || message.Contains("stats") || message.Contains("platform data"))
            {
                return await GetPlatformStatistics();
            }
            
            if (message.Contains("hello") || message.Contains("hi") || message.Contains("greeting"))
            {
                return await GetGreetingResponse(contextData);
            }
            
            return await GetDefaultResponse();
        }
        catch (Exception ex)
        {
            return "I'm having trouble accessing the platform data right now. Please try again later or contact support.";
        }
    }

    private async Task<object> GetPlatformDataForContext()
    {
        try
        {
            // Get available data from analytics service
            var platformOverview = await _serviceManager.AnalyticsService.GetPlatformOverviewAsync();
            return platformOverview;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting platform data for context");
            return new { error = "Unable to fetch platform data" };
        }
    }

    private async Task<string> GetTopSellersInfo()
    {
        try
        {
            // Get top sellers from the analytics service
            var topSellers = await _serviceManager.AnalyticsService.GetTopSellersAsync(5);
            
            if (!topSellers.Any())
            {
                return "Currently, there are no sellers with sufficient activity to determine top performers. As more sellers join and list products, we'll be able to provide top seller rankings.";
            }
            
            var response = "Here are some of the top sellers on our platform based on their activity and product listings:\n\n";
            
            foreach (var seller in topSellers)
            {
                response += $"• {seller.UserName} - {seller.Email}\n";
            }
            
            response += $"\nTotal top sellers found: {topSellers.Count()}";
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting top sellers info");
            return "I can help you find top sellers on our platform. The system tracks sellers based on their product listings and activity. Would you like me to provide more specific information about any particular seller?";
        }
    }

    private async Task<string> GetPopularProductsInfo()
    {
        try
        {
            // Get top selling products from the analytics service
            var topProducts = await _serviceManager.AnalyticsService.GetTopSellingProductsAsync(5);
            
            if (!topProducts.Any())
            {
                return "Currently, there are no products listed on the platform. Products will appear here once sellers start listing them. You can be the first to list a product!";
            }
            
            var response = "Here are some popular products on our platform:\n\n";
            
            foreach (var product in topProducts)
            {
                var description = product.Description?.Length > 50 
                    ? product.Description.Substring(0, 50) + "..." 
                    : product.Description ?? "No description available";
                response += $"• **{product.Name}** - {description}\n";
            }
            
            response += $"\nTotal top products found: {topProducts.Count()}";
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting popular products info");
            return "I can see the most popular products on our platform. These are determined by various factors including views, inquiries, and listings. Would you like me to provide details about specific products?";
        }
    }

    private async Task<string> GetLiveAuctionsInfo()
    {
        try
        {
            // Get live auctions from the analytics service
            var liveAuctions = await _serviceManager.AnalyticsService.GetLiveAuctionsAsync();
            
            if (!liveAuctions.Any())
            {
                return "Currently, there are no live auctions running on the platform. Check back later for new auction opportunities! You can also create an auction if you have products to sell.";
            }
            
            var response = $"There are currently **{liveAuctions.Count()}** live auctions running on the platform:\n\n";
            
            foreach (var auction in liveAuctions.Take(3))
            {
                response += $"• **{auction.Name}** - Ends: {auction.EndTime:MMM dd, yyyy HH:mm}\n";
            }
            
            if (liveAuctions.Count() > 3)
            {
                response += $"\n... and {liveAuctions.Count() - 3} more auctions";
            }
            
            response += $"\n\nTotal live auctions: {liveAuctions.Count()}";
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting live auctions info");
            return "I can see there are live auctions currently running on the platform. These auctions allow buyers to bid on products. Would you like information about specific auctions or how the auction process works?";
        }
    }

    private async Task<string> GetPopularServicesInfo()
    {
        try
        {
            // Get top used services from the analytics service
            var topServices = await _serviceManager.AnalyticsService.GetTopUsedServicesAsync(5);
            
            if (!topServices.Any())
            {
                return "Currently, there are no services listed on the platform. Services will appear here once providers start listing them. You can be the first to offer a service!";
            }
            
            var response = "Here are some popular services on our platform:\n\n";
            
            foreach (var service in topServices)
            {
                var description = service.Description?.Length > 50 
                    ? service.Description.Substring(0, 50) + "..." 
                    : service.Description ?? "No description available";
                response += $"• **{service.Name}** - {description}\n";
            }
            
            response += $"\nTotal top services: {topServices.Count()}";
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting popular services info");
            return "Our platform offers various services from different providers. I can provide information about the most popular services currently available. What type of service are you looking for?";
        }
    }

    private async Task<string> GetCategoriesInfo()
    {
        try
        {
            // Get categories from the analytics service
            var categoryStats = await _serviceManager.AnalyticsService.GetCategoryStatsAsync();
            
            if (!categoryStats.Any())
            {
                return "Currently, there are no product categories set up on the platform. Categories will be added as the platform grows.";
            }
            
            var response = "Here are the product categories available on our platform:\n\n";
            
            foreach (var category in categoryStats)
            {
                response += $"• **{category.CategoryName}**\n";
            }
            
            response += $"\nTotal categories: {categoryStats.Count()}";
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting categories info");
            return "I can help you explore different product categories on our platform. Each category contains various products from different sellers. Which category interests you?";
        }
    }

    private async Task<string> GetPlatformStatistics()
    {
        try
        {
            var topProducts = await _serviceManager.AnalyticsService.GetTopSellingProductsAsync(10);
            var liveAuctions = await _serviceManager.AnalyticsService.GetLiveAuctionsAsync();
            var categoryStats = await _serviceManager.AnalyticsService.GetCategoryStatsAsync();
            var topServices = await _serviceManager.AnalyticsService.GetTopUsedServicesAsync(10);
            
            var response = "Here are the current platform statistics:\n\n";
            response += $"• **Top Products**: {topProducts.Count()}\n";
            response += $"• **Live Auctions**: {liveAuctions.Count()}\n";
            response += $"• **Categories**: {categoryStats.Count()}\n";
            response += $"• **Top Services**: {topServices.Count()}\n";
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting platform statistics");
            return "I can provide you with platform statistics including product listings, services, and auction information. What specific data would you like to see?";
        }
    }

    private async Task<string> GetHelpInfo()
    {
        var response = "Hello! I'm your **Stockat AI assistant**. I can help you with:\n\n";
        response += "• **Top Sellers** - Find the best sellers on our platform\n";
        response += "• **Popular Products** - Discover trending products\n";
        response += "• **Live Auctions** - Check current auction status\n";
        response += "• **Services** - Explore available services\n";
        response += "• **Categories** - Browse product categories\n";
        response += "• **Statistics** - Get platform data and metrics\n\n";
        response += "Just ask me about any of these topics!";
        
        return response;
    }

    private async Task<string> GetGreetingResponse(object contextData)
    {
        var userEmail = "";
        var userRole = "";
        
        try
        {
            // Extract user information from context data
            var contextJson = JsonSerializer.Serialize(contextData);
            var contextDict = JsonSerializer.Deserialize<Dictionary<string, object>>(contextJson);
            
            if (contextDict != null)
            {
                contextDict.TryGetValue("userEmail", out var email);
                contextDict.TryGetValue("userRole", out var role);
                
                userEmail = email?.ToString() ?? "";
                userRole = role?.ToString() ?? "";
            }
        }
        catch
        {
            // Ignore errors in context parsing
        }

        var greeting = "Hello! 👋 ";
        
        if (!string.IsNullOrEmpty(userEmail))
        {
            var name = userEmail.Split('@')[0];
            greeting += $"Welcome back, {name}! ";
        }
        
        greeting += "I'm your **Stockat AI assistant**. I can help you find information about products, services, sellers, auctions, and more on our B2B manufacturing platform. What would you like to know?";
        
        return greeting;
    }

    private async Task<string> GetDefaultResponse()
    {
        return "Hello! I'm your **Stockat AI assistant**. I can help you find information about products, services, sellers, auctions, and more on our B2B manufacturing platform. Try asking me about:\n\n" +
               "• **Top sellers** and popular products\n" +
               "• **Live auctions** and bidding\n" +
               "• **Available services** and categories\n" +
               "• **Platform statistics**\n\n" +
               "What would you like to know?";
    }
} 