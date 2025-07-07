using Stockat.Core.DTOs;
using Stockat.Core.DTOs.ProductDTOs;
using Stockat.Core.DTOs.ServiceDTOs;
using Stockat.Core.DTOs.UserDTOs;
using Stockat.Core.DTOs.CategoryDtos;
using Stockat.Core.DTOs.AuctionDTOs;

namespace Stockat.Core.IServices;

public interface IAnalyticsService
{
    // Top performers
    Task<IEnumerable<UserReadDto>> GetTopSellersAsync(int limit = 5);
    Task<IEnumerable<ProductDetailsDto>> GetTopSellingProductsAsync(int count = 5);
    Task<IEnumerable<ServiceDto>> GetTopUsedServicesAsync(int count = 5);
    
    // Platform statistics
    Task<IEnumerable<AuctionDetailsForChatbot>> GetLiveAuctionsAsync();
    Task<IEnumerable<CategoryDto>> GetCategoryStatsAsync();
    
    // Platform overview
    Task<object> GetPlatformOverviewAsync();
} 