using AutoMapper;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.ProductDTOs;
using Stockat.Core.DTOs.ServiceDTOs;
using Stockat.Core.DTOs.UserDTOs;
using Stockat.Core.DTOs.CategoryDtos;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.IServices;
using Stockat.Core.Enums;
using Stockat.Core.Entities;

namespace Stockat.Service.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly ILoggerManager _logger;
    private readonly IServiceManager _serviceManager;

    public AnalyticsService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        ILoggerManager logger,
        IServiceManager serviceManager)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _logger = logger;
        _serviceManager = serviceManager;
    }

    public async Task<IEnumerable<UserReadDto>> GetTopSellersAsync(int limit = 5)
    {
        try
        {
            _logger.LogInfo("GetTopSellersAsync: Starting comprehensive top sellers calculation");

            // 1. Get product orders (completed sales)
            var productOrders = await _repositoryManager.OrderRepo.FindAllAsync(
                op => op.OrderType == OrderType.Order && 
                      op.Status == OrderStatus.Completed &&
                      op.Seller.IsDeleted == false &&
                      op.Seller.IsApproved == true,
                includes: ["Seller", "Product"]
            );

            // 2. Get service requests (completed service sales)
            var serviceRequests = await _repositoryManager.ServiceRequestRepo.FindAllAsync(
                sr => sr.ServiceStatus == ServiceStatus.Delivered &&
                      sr.PaymentStatus == PaymentStatus.Paid &&
                      sr.Service.Seller.IsDeleted == false &&
                      sr.Service.Seller.IsApproved == true,
                includes: ["Service", "Service.Seller"]
            );

            // 3. Get product requests (custom product requests)
            var productRequests = await _repositoryManager.OrderRepo.FindAllAsync(
                op => op.OrderType == OrderType.Request && 
                      op.Status == OrderStatus.Completed &&
                      op.Seller.IsDeleted == false &&
                      op.Seller.IsApproved == true,
                includes: ["Seller", "Product"]
            );

            _logger.LogInfo($"GetTopSellersAsync: Found {productOrders.Count()} product orders, {serviceRequests.Count()} service requests, {productRequests.Count()} product requests");

            // 4. Combine all seller activities
            var sellerActivities = new Dictionary<string, SellerActivity>();

            // Process product orders
            foreach (var order in productOrders)
            {
                if (!sellerActivities.ContainsKey(order.SellerId))
                {
                    sellerActivities[order.SellerId] = new SellerActivity
                    {
                        Seller = order.Seller,
                        ProductOrders = 0,
                        ProductRequests = 0,
                        ServiceRequests = 0,
                        TotalRevenue = 0,
                        TotalProducts = 0,
                        TotalServices = 0
                    };
                }

                sellerActivities[order.SellerId].ProductOrders++;
                sellerActivities[order.SellerId].TotalRevenue += order.Price * order.Quantity;
                sellerActivities[order.SellerId].TotalProducts++;
            }

            // Process service requests
            foreach (var request in serviceRequests)
            {
                var sellerId = request.Service.SellerId;
                if (!sellerActivities.ContainsKey(sellerId))
                {
                    sellerActivities[sellerId] = new SellerActivity
                    {
                        Seller = request.Service.Seller,
                        ProductOrders = 0,
                        ProductRequests = 0,
                        ServiceRequests = 0,
                        TotalRevenue = 0,
                        TotalProducts = 0,
                        TotalServices = 0
                    };
                }

                sellerActivities[sellerId].ServiceRequests++;
                sellerActivities[sellerId].TotalRevenue += request.TotalPrice;
                sellerActivities[sellerId].TotalServices++;
            }

            // Process product requests
            foreach (var request in productRequests)
            {
                if (!sellerActivities.ContainsKey(request.SellerId))
                {
                    sellerActivities[request.SellerId] = new SellerActivity
                    {
                        Seller = request.Seller,
                        ProductOrders = 0,
                        ProductRequests = 0,
                        ServiceRequests = 0,
                        TotalRevenue = 0,
                        TotalProducts = 0,
                        TotalServices = 0
                    };
                }

                sellerActivities[request.SellerId].ProductRequests++;
                sellerActivities[request.SellerId].TotalRevenue += request.Price * request.Quantity;
                sellerActivities[request.SellerId].TotalProducts++;
            }

            // 5. Calculate seller scores and rank them
            var rankedSellers = sellerActivities.Values
                .Select(sa => new
                {
                    Seller = sa.Seller,
                    Score = CalculateSellerScore(sa),
                    TotalRevenue = sa.TotalRevenue,
                    TotalOrders = sa.ProductOrders + sa.ProductRequests + sa.ServiceRequests,
                    ProductOrders = sa.ProductOrders,
                    ProductRequests = sa.ProductRequests,
                    ServiceRequests = sa.ServiceRequests
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.TotalRevenue)
                .ThenByDescending(x => x.TotalOrders)
                .Take(limit)
                .Select(x => x.Seller)
                .ToList();

            _logger.LogInfo($"GetTopSellersAsync: Ranked {rankedSellers.Count} top sellers based on comprehensive metrics");
            
            var userDtos = new List<UserReadDto>();
            
            foreach (var user in rankedSellers)
            {
                var userDto = _mapper.Map<UserReadDto>(user);
                userDto.IsApproved = user.IsApproved;
                userDto.IsDeleted = user.IsDeleted;
                
                userDtos.Add(userDto);
            }
            
            _logger.LogInfo($"GetTopSellersAsync: Returning {userDtos.Count} mapped user DTOs");
            return userDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting top sellers: {ex.Message}");
            
            // Fallback to product count method if there's an error with sales data
            try
            {
                _logger.LogInfo("GetTopSellersAsync: Falling back to product count method");
                var topSellers = await _repositoryManager.UserRepo.GetTopSellersAsync(limit);
                
                var userDtos = new List<UserReadDto>();
                
                foreach (var user in topSellers)
                {
                    var userDto = _mapper.Map<UserReadDto>(user);
                    userDto.IsApproved = user.IsApproved;
                    userDto.IsDeleted = user.IsDeleted;
                    
                    userDtos.Add(userDto);
                }
                
                _logger.LogInfo($"GetTopSellersAsync: Fallback returning {userDtos.Count} sellers based on product count");
                return userDtos;
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError($"Error in fallback for top sellers: {fallbackEx.Message}");
                return new List<UserReadDto>();
            }
        }
    }

    private double CalculateSellerScore(SellerActivity activity)
    {
        // Weighted scoring system:
        // - Revenue: 40% weight
        // - Total orders: 30% weight  
        // - Product diversity: 20% weight (products + services)
        // - Order diversity: 10% weight (different types of orders)

        var totalOrders = activity.ProductOrders + activity.ProductRequests + activity.ServiceRequests;
        var orderDiversity = 0;
        
        if (activity.ProductOrders > 0) orderDiversity++;
        if (activity.ProductRequests > 0) orderDiversity++;
        if (activity.ServiceRequests > 0) orderDiversity++;

        var revenueScore = Math.Min((double)activity.TotalRevenue / 10000.0, 1.0) * 40; // Normalize to 0-40
        var ordersScore = Math.Min(totalOrders / 50.0, 1.0) * 30; // Normalize to 0-30
        var diversityScore = Math.Min((activity.TotalProducts + activity.TotalServices) / 20.0, 1.0) * 20; // Normalize to 0-20
        var orderTypeScore = (orderDiversity / 3.0) * 10; // 0-10 based on order types

        return revenueScore + ordersScore + diversityScore + orderTypeScore;
    }

    private class SellerActivity
    {
        public User Seller { get; set; }
        public int ProductOrders { get; set; }
        public int ProductRequests { get; set; }
        public int ServiceRequests { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalProducts { get; set; }
        public int TotalServices { get; set; }
    }

    public async Task<IEnumerable<ProductDetailsDto>> GetTopSellingProductsAsync(int count = 5)
    {
        try
        {
            // Get top-selling products based on order data
            var topSellingProducts = await _repositoryManager.OrderRepo.FindAllAsync(
                op => op.OrderType == Core.Enums.OrderType.Order && 
                      op.Status != Core.Enums.OrderStatus.Cancelled &&
                      op.Product.isDeleted == false &&
                      (op.Product.ProductStatus == Core.Enums.ProductStatus.Approved || 
                       op.Product.ProductStatus == Core.Enums.ProductStatus.Activated),
                includes: ["Product", "Product.Images", "Product.Category", "Product.User"]
            );

            // Group by product and calculate total sales
            var productSales = topSellingProducts
                .GroupBy(op => op.ProductId)
                .Select(g => new
                {
                    Product = g.First().Product,
                    TotalQuantity = g.Sum(op => op.Quantity),
                    TotalRevenue = g.Sum(op => op.Price * op.Quantity),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ThenByDescending(x => x.TotalRevenue)
                .Take(count)
                .Select(x => x.Product)
                .ToList();

            _logger.LogInfo($"GetTopSellingProductsAsync: Found {productSales.Count()} top-selling products based on order data");
            
            var result = _mapper.Map<IEnumerable<ProductDetailsDto>>(productSales);
            
            _logger.LogInfo($"GetTopSellingProductsAsync: Returning {result.Count()} mapped product DTOs");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting top selling products: {ex.Message}");
            
            // Fallback to recent products if there's an error with order data
            try
            {
                _logger.LogInfo("GetTopSellingProductsAsync: Falling back to recent products");
                var recentProducts = await _repositoryManager.ProductRepository.FindAllAsync(
                    p => p.isDeleted == false && 
                         (p.ProductStatus == Core.Enums.ProductStatus.Approved || p.ProductStatus == Core.Enums.ProductStatus.Activated),
                    skip: 0,
                    take: count,
                    includes: ["Images", "Category"],
                    orderBy: p => p.Id,
                    orderByDirection: "DESC"
                );

                var fallbackResult = _mapper.Map<IEnumerable<ProductDetailsDto>>(recentProducts);
                _logger.LogInfo($"GetTopSellingProductsAsync: Fallback returning {fallbackResult.Count()} recent products");
                return fallbackResult;
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError($"Error in fallback for top selling products: {fallbackEx.Message}");
                return new List<ProductDetailsDto>();
            }
        }
    }

    public async Task<IEnumerable<ServiceDto>> GetTopUsedServicesAsync(int count = 5)
    {
        try
        {
            // Use the existing ServiceService method to get top services
            var topServicesResponse = await _serviceManager.ServiceService.GetTopServicesAsync();
            
            if (topServicesResponse.Status != 200 || topServicesResponse.Data == null)
            {
                _logger.LogWarn("GetTopUsedServicesAsync: ServiceService returned no data, falling back to basic service list");
                return await GetFallbackServicesAsync(count);
            }

            // Convert the dynamic objects to Service entities
            var topServices = new List<Stockat.Core.Entities.Service>();
            
            foreach (var serviceData in topServicesResponse.Data)
            {
                // Get the service by ID from the repository
                var serviceId = (int)serviceData.GetType().GetProperty("ServiceId").GetValue(serviceData);
                var service = await _repositoryManager.ServiceRepo.FindAsync(s => s.Id == serviceId, includes: ["Seller"]);
                
                if (service != null)
                {
                    topServices.Add(service);
                }
            }

            _logger.LogInfo($"GetTopUsedServicesAsync: Found {topServices.Count} top services using ServiceService method");
            
            return _mapper.Map<IEnumerable<ServiceDto>>(topServices);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting top used services: {ex.Message}");
            return await GetFallbackServicesAsync(count);
        }
    }

    private async Task<IEnumerable<ServiceDto>> GetFallbackServicesAsync(int count)
    {
        try
        {
            _logger.LogInfo("GetTopUsedServicesAsync: Using fallback method");
            
            // Fallback to basic approved services
            var services = await _repositoryManager.ServiceRepo.FindAllAsync(
                s => s.IsApproved == ApprovalStatus.Approved && !s.IsDeleted,
                skip: 0,
                take: count,
                includes: ["Seller"],
                orderBy: s => s.CreatedAt,
                orderByDirection: "DESC"
            );

            var result = _mapper.Map<IEnumerable<ServiceDto>>(services);
            _logger.LogInfo($"GetTopUsedServicesAsync: Fallback returning {result.Count()} services");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in fallback for top services: {ex.Message}");
            return new List<ServiceDto>();
        }
    }



    public async Task<IEnumerable<AuctionDetailsForChatbot>> GetLiveAuctionsAsync()
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

            return _mapper.Map<IEnumerable<AuctionDetailsForChatbot>>(liveAuctions);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting live auctions: {ex.Message}");
            return new List<AuctionDetailsForChatbot>();
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