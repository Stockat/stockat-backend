using Stockat.Core.Entities;

namespace Stockat.Core.IRepositories
{
    public interface IReviewRepository : IBaseRepository<Review>
    {
        Task<IEnumerable<Review>> GetProductReviewsAsync(int productId, int skip, int take);
        Task<IEnumerable<Review>> GetServiceReviewsAsync(int serviceId, int skip, int take);
        Task<IEnumerable<Review>> GetUserReviewsAsync(string userId, int skip, int take);
        Task<Review?> GetReviewByOrderProductAsync(int orderProductId, string userId);
        Task<Review?> GetReviewByServiceRequestAsync(int serviceRequestId, string userId);
        Task<bool> HasUserReviewedProductAsync(int orderProductId, string userId);
        Task<bool> HasUserReviewedServiceAsync(int serviceRequestId, string userId);
        Task<double> GetProductAverageRatingAsync(int productId);
        Task<double> GetServiceAverageRatingAsync(int serviceId);
        Task<int> GetProductReviewCountAsync(int productId);
        Task<int> GetServiceReviewCountAsync(int serviceId);
        Task<Dictionary<int, int>> GetProductRatingDistributionAsync(int productId);
        Task<Dictionary<int, int>> GetServiceRatingDistributionAsync(int serviceId);
    }
} 