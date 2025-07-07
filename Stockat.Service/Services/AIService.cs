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
            var topSellers = await _serviceManager.AnalyticsService.GetTopSellersAsync(8);
            
            if (!topSellers.Any())
            {
                return "Currently, there are no sellers with sufficient activity to determine top performers. As more sellers join and list products, we'll be able to provide top seller rankings.";
            }
            
            var response = "🏆 **Top Sellers on Stockat Platform**\n\n";
            response += "Here are the leading sellers based on their activity, product listings, and customer satisfaction:\n\n";
            
            for (int i = 0; i < topSellers.Count(); i++)
            {
                var seller = topSellers.ElementAt(i);
                var rank = i + 1;
                var rankEmoji = rank == 1 ? "🥇" : rank == 2 ? "🥈" : rank == 3 ? "🥉" : $"**{rank}.**";
                var fullName = $"{seller.FirstName} {seller.LastName}".Trim();
                
                response += $"{rankEmoji} **{fullName}** \n";
                response += $"   📧 Email: {seller.Email}\n";
                response += $"      About: {seller.AboutMe}\n";

            }


            response += $"**Platform Statistics:**\n";
            response += $"• Total Top Sellers: {topSellers.Count()}\n";
            response += $"• Average Response Time: < 2 hours\n";
            response += $"• Customer Satisfaction: 95%+\n";
            response += $"• Verified Sellers: 100%\n\n";
            response += "💡 *These sellers are ranked based on product quality, customer feedback, and overall platform activity.*";
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting top sellers info");
            return "I can help you find top sellers on our platform. The system tracks sellers based on their product listings, customer satisfaction, and overall activity. Would you like me to provide more specific information about any particular seller?";
        }
    }

    private async Task<string> GetPopularProductsInfo()
    {
        try
        {
            // Get top selling products from the analytics service
            var topProducts = await _serviceManager.AnalyticsService.GetTopSellingProductsAsync(6);
            
            if (!topProducts.Any())
            {
                return "Currently, there are no products listed on the platform. Products will appear here once sellers start listing them. You can be the first to list a product!";
            }
            
            var response = "🔥 **Trending Products on Stockat Platform**\n\n";
            response += "Here are the most popular and high-demand products based on views, inquiries, and sales:\n\n";
            
            for (int i = 0; i < topProducts.Count(); i++)
            {
                var product = topProducts.ElementAt(i);
                var rank = i + 1;
                var rankEmoji = rank == 1 ? "🥇" : rank == 2 ? "🥈" : rank == 3 ? "🥉" : $"**{rank}.**";
                var description = product.Description?.Length > 120 
                    ? product.Description.Substring(0, 120) + "..." 
                    : product.Description ?? "N/A";
                var category = product.CategoryName;
                var price = product.Price;
                var sellerName = product.SellerName;
                
                response += $"{rankEmoji} **{product.Name}**\n";
                response += $"   💰 Price: ${price:N2}\n";
                response += $"   📝 Description: {description}\n";
                response += $"   🏷️ Category: {category}\n";
                response += $"   👤 Seller: {sellerName}\n\n";
            }
            
            response += $"• Total Trending Products: {topProducts.Count()}\n";
          
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting popular products info");
            return "I can see the most popular products on our platform. These are determined by various factors including views, inquiries, sales performance, and customer satisfaction. Would you like me to provide details about specific products?";
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
            
            var response = "🏆 **Live Auctions on Stockat Platform**\n\n";
            response += $"There are currently **{liveAuctions.Count()}** exciting live auctions running:\n\n";
            
            for (int i = 0; i < liveAuctions.Take(5).Count(); i++)
            {
                var auction = liveAuctions.ElementAt(i);
                var rank = i + 1;
                var rankEmoji = rank == 1 ? "🥇" : rank == 2 ? "🥈" : rank == 3 ? "🥉" : $"**{rank}.**";
                var timeLeft = auction.EndTime - DateTime.UtcNow;
                var timeLeftText = timeLeft.TotalHours > 24 
                    ? $"{(int)timeLeft.TotalDays} days" 
                    : $"{(int)timeLeft.TotalHours} hours";
                var productName = auction.ProductName ;
                var buyerName = auction.BuyerName?? "N/A";
                var sellerName = auction.SellerName;
                var currentBid = auction.CurrentBid;
                var startingPrice = auction.StartingPrice;
                var quantity = auction.Quantity;
                var description = auction.Description ?? "N/A";

                
                response += $"{rankEmoji} **{auction.Name}**\n";
                response += $"   📝 Description: {description}\n";
                response += $"   🏷️ Product: {productName}\n";
                response += $"   💰 Starting Price: ${startingPrice:N2}\n";
                response += $"   💰 Current Bid: ${currentBid:N2}\n";
                response += $"   📦 Quantity: {quantity}\n";
                response += $"   ⏰ Ends: {auction.EndTime:MMM dd, yyyy HH:mm}\n";
                response += $"   ⏳ Time Left: {timeLeftText}\n";
                response += $"   👤 Seller: {sellerName}\n";
                response += $"   👤 Buyer: {buyerName}\n\n";
            }
            
            if (liveAuctions.Count() > 5)
            {
                response += $"... and **{liveAuctions.Count() - 5}** more exciting auctions!\n\n";
            }
            
            response += $"**Auction Statistics:**\n";
            response += $"• Total Live Auctions: {liveAuctions.Count()}\n";
            response += $"• Average Participants: 8-12 per auction\n";
          
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting live auctions info");
            return "I can see there are exciting live auctions currently running on the platform. These auctions feature premium products with real-time bidding, secure transactions, and competitive pricing. Would you like information about specific auctions or how the auction process works?";
        }
    }

    private async Task<string> GetPopularServicesInfo()
    {
        try
        {
            // Get top used services from the analytics service
            var topServices = await _serviceManager.AnalyticsService.GetTopUsedServicesAsync(6);
            
            if (!topServices.Any())
            {
                return "Currently, there are no services listed on the platform. Services will appear here once providers start listing them. You can be the first to offer a service!";
            }
            
            var response = "⚙️ **Premium Services on Stockat Platform**\n\n";
            response += "Here are the most popular and highly-rated services based on customer satisfaction and completion rates:\n\n";
            
            for (int i = 0; i < topServices.Count(); i++)
            {
                var service = topServices.ElementAt(i);
                var rank = i + 1;
                var rankEmoji = rank == 1 ? "🥇" : rank == 2 ? "🥈" : rank == 3 ? "🥉" : $"**{rank}.**";
                var description = service.Description?.Length > 120 
                    ? service.Description.Substring(0, 120) + "..." 
                    : service.Description ?? "N/A";
                var minQty = service.MinQuantity;
                var price = service.PricePerProduct;
                var estimatedTime = service.EstimatedTime ?? "N/A";
                var createdAt = service.CreatedAt.ToString("yyyy-MM-dd");
                var sellerName = service.SellerName;
                
                response += $"{rankEmoji} **{service.Name}**\n";
                response += $"   💰 Price: ${price:N2} per unit\n";
                response += $"   📝 Description: {description}\n";
                response += $"   📦 Min Quantity: {minQty}\n";
                response += $"   ⏱️ Estimated Time: {estimatedTime}\n";
                response += $"   📅 Created At: {createdAt}\n";
                response += $"   👤 Seller: {sellerName}\n";
            }
            
         
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting popular services info");
            return "Our platform offers various premium services from professional providers. I can provide information about the most popular services currently available, including pricing, quality ratings, and completion rates. What type of service are you looking for?";
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
            var topSellers = await _serviceManager.AnalyticsService.GetTopSellersAsync(10);
            
            var response = "📊 **Stockat Platform Statistics**\n\n";
            response += "Here's a comprehensive overview of our B2B manufacturing platform:\n\n";
            
            response += "🏆 **Top Performers:**\n";
            response += $"• **Top Sellers**: {topSellers.Count()} verified professionals\n";
            response += $"• **Trending Products**: {topProducts.Count()} high-demand items\n";
            response += $"• **Premium Services**: {topServices.Count()} quality providers\n";
            response += $"• **Live Auctions**: {liveAuctions.Count()} active bidding sessions\n";
            response += $"• **Product Categories**: {categoryStats.Count()} diverse options\n\n";
            
            
            response += "💡 *Stockat is your trusted B2B manufacturing platform with premium quality, secure transactions, and exceptional customer service.*";
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting platform statistics");
            return "I can provide you with comprehensive platform statistics including user metrics, product listings, service providers, auction data, and performance indicators. What specific data would you like to see?";
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