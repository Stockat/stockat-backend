using AutoMapper;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.ProductDTOs;
using Stockat.Core.DTOs.ServiceDTOs;
using Stockat.Core.DTOs.UserDTOs;
using Stockat.Core.DTOs.CategoryDtos;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.IServices;

namespace Stockat.Service.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly ILoggerManager _logger;

    public AnalyticsService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        ILoggerManager logger)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<UserReadDto>> GetTopSellersAsync(int limit = 5)
    {
        try
        {
            // Get users who have products (sellers) and order by product count
            var topSellers = await _repositoryManager.UserRepo.GetTopSellersAsync(limit);
            
            var userDtos = new List<UserReadDto>();
            
            foreach (var user in topSellers)
            {
                var userDto = _mapper.Map<UserReadDto>(user);
                userDto.IsApproved = user.IsApproved;
                userDto.IsDeleted = user.IsDeleted;
                
                userDtos.Add(userDto);
            }
            
            return userDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting top sellers: {ex.Message}");
            return new List<UserReadDto>();
        }
    }

    public async Task<IEnumerable<ProductHomeDto>> GetTopSellingProductsAsync(int count = 5)
    {
        try
        {
            var topProducts = await _repositoryManager.ProductRepository.FindAllAsync(
                p => p.isDeleted == false && 
                     (p.ProductStatus == Core.Enums.ProductStatus.Approved || p.ProductStatus == Core.Enums.ProductStatus.Activated),
                skip: 0,
                take: count,
                includes: ["Images", "Category"],
                orderBy: p => p.Price, // Simplified - in real scenario, use actual sales data
                orderByDirection: "DESC"
            );

            return _mapper.Map<IEnumerable<ProductHomeDto>>(topProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting top selling products: {ex.Message}");
            return new List<ProductHomeDto>();
        }
    }

    public async Task<IEnumerable<ServiceDto>> GetTopUsedServicesAsync(int count = 5)
    {
        try
        {
            // Get all approved services with their related data
            var services = await _repositoryManager.ServiceRepo.FindAllAsync(
                s => s.IsApproved,
                skip: 0,
                take: 100, // Get more to calculate rankings
                includes: ["Category", "Seller", "ServiceRequests"]
            );

            if (!services.Any())
            {
                return new List<ServiceDto>();
            }

            // Calculate service scores based on multiple factors
            var serviceScores = new List<(Stockat.Core.Entities.Service, double Score)>();

            foreach (Stockat.Core.Entities.Service service in services)
            {
                var score = await CalculateServiceScore(service);
                serviceScores.Add((service, score));
            }

            // Sort by score and take top services
            var topServices = serviceScores
                .OrderByDescending(x => x.Score)
                .Take(count);
                //.Select(x => x.Service);

            return _mapper.Map<IEnumerable<ServiceDto>>(topServices);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting top used services: {ex.Message}");
            return new List<ServiceDto>();
        }
    }

    private async Task<double> CalculateServiceScore(Stockat.Core.Entities.Service service)
    {
        try
        {
            // Get all service requests for this service
            var serviceRequests = await _repositoryManager.ServiceRequestRepo.FindAllAsync(
                sr => sr.ServiceId == service.Id
            );

            if (!serviceRequests.Any())
            {
                return 0; // No requests = no score
            }

            // Factor 1: Request Count (30% weight)
            var requestCount = serviceRequests.Count();
            var requestCountScore = Math.Min(requestCount / 10.0, 1.0) * 30; // Normalize to 0-30

            // Factor 2: Approval Rate (25% weight)
            var approvedRequests = serviceRequests.Count(sr => 
                sr.SellerApprovalStatus == Core.Enums.ApprovalStatus.Approved && 
                sr.BuyerApprovalStatus == Core.Enums.ApprovalStatus.Approved);
            var approvalRate = requestCount > 0 ? (double)approvedRequests / requestCount : 0;
            var approvalRateScore = approvalRate * 25;

            // Factor 3: Average Request Value (20% weight)
            var totalValue = serviceRequests.Sum(sr => sr.TotalPrice);
            var averageValue = requestCount > 0 ? totalValue / requestCount : 0;
            var averageValueScore = Math.Min((double)averageValue / 1000.0, 1.0) * 20; // Normalize to 0-20

            // Factor 4: Recent Activity (15% weight)
            var recentRequests = serviceRequests.Count(sr => 
                sr.CreatedAt >= DateTime.UtcNow.AddDays(-30)); // Last 30 days
            var recentActivityScore = Math.Min(recentRequests / 5.0, 1.0) * 15; // Normalize to 0-15

            // Factor 5: Service Quality (10% weight)
            // Based on completion rate and payment status
            var completedRequests = serviceRequests.Count(sr => 
                sr.ServiceStatus == Core.Enums.ServiceStatus.Delivered);
            var qualityScore = requestCount > 0 ? (double)completedRequests / requestCount * 10 : 0;

            // Calculate total score
            var totalScore = requestCountScore + approvalRateScore + averageValueScore + 
                           recentActivityScore + qualityScore;

            return totalScore;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calculating service score for service {service.Id}: {ex.Message}");
            return 0;
        }
    }

    public async Task<IEnumerable<AuctionDetailsDto>> GetLiveAuctionsAsync()
    {
        try
        {
            var liveAuctions = await _repositoryManager.AuctionRepo.FindAllAsync(
                a => !a.IsClosed && a.EndTime > DateTime.UtcNow,
                skip: 0,
                take: 10,
                includes: ["Product", "SellerUser"],
                orderBy: a => a.EndTime,
                orderByDirection: "ASC"
            );

            return _mapper.Map<IEnumerable<AuctionDetailsDto>>(liveAuctions);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting live auctions: {ex.Message}");
            return new List<AuctionDetailsDto>();
        }
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoryStatsAsync()
    {
        try
        {
            var categories = await _repositoryManager.CategoryRepo.FindAllAsync(
                c => c.Products.Any(p => p.isDeleted == false),
                skip: 0,
                take: 10,
                includes: ["Products"],
                orderBy: c => c.Products.Count,
                orderByDirection: "DESC"
            );

            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting category stats: {ex.Message}");
            return new List<CategoryDto>();
        }
    }

    public async Task<object> GetPlatformOverviewAsync()
    {
        try
        {
            var topProducts = await GetTopSellingProductsAsync(10);
            var liveAuctions = await GetLiveAuctionsAsync();
            var categoryStats = await GetCategoryStatsAsync();
            var topSellers = await GetTopSellersAsync(5);

            return new
            {
                platformStats = new
                {
                    topProductsCount = topProducts.Count(),
                    liveAuctionsCount = liveAuctions.Count(),
                    categoriesCount = categoryStats.Count(),
                    topSellersCount = topSellers.Count()
                },
                topProducts = topProducts.Select(p => new { p.Name, p.Description }),
                liveAuctions = liveAuctions.Take(3).Select(a => new { a.Name, a.EndTime }),
                categories = categoryStats.Select(c => new { c.CategoryName }),
                topSellers = topSellers.Select(s => new { s.UserName, s.Email })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting platform overview: {ex.Message}");
            return new { error = "Unable to fetch platform data" };
        }
    }
} 