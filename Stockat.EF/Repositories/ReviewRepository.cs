using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using Stockat.Core.IRepositories;

namespace Stockat.EF.Repositories
{
    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        public ReviewRepository(StockatDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Review>> GetProductReviewsAsync(int productId, int skip, int take)
        {
            return await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.Reviewer)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetServiceReviewsAsync(int serviceId, int skip, int take)
        {
            return await _context.Reviews
                .Where(r => r.ServiceId == serviceId)
                .Include(r => r.Reviewer)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetUserReviewsAsync(string userId, int skip, int take)
        {
            return await _context.Reviews
                .Where(r => r.ReviewerId == userId)
                .Include(r => r.Product)
                .Include(r => r.Service)
                .Include(r => r.OrderProduct)
                .Include(r => r.ServiceRequest)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Review?> GetReviewByOrderProductAsync(int orderProductId, string userId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.OrderProductId == orderProductId && r.ReviewerId == userId);
        }

        public async Task<Review?> GetReviewByServiceRequestAsync(int serviceRequestId, string userId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.ServiceRequestId == serviceRequestId && r.ReviewerId == userId);
        }

        public async Task<bool> HasUserReviewedProductAsync(int orderProductId, string userId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.OrderProductId == orderProductId && r.ReviewerId == userId);
        }

        public async Task<bool> HasUserReviewedServiceAsync(int serviceRequestId, string userId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.ServiceRequestId == serviceRequestId && r.ReviewerId == userId);
        }

        public async Task<double> GetProductAverageRatingAsync(int productId)
        {
            var average = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .AverageAsync(r => (double)r.Rating);
            return Math.Round(average, 1);
        }

        public async Task<double> GetServiceAverageRatingAsync(int serviceId)
        {
            var average = await _context.Reviews
                .Where(r => r.ServiceId == serviceId)
                .AverageAsync(r => (double)r.Rating);
            return Math.Round(average, 1);
        }

        public async Task<int> GetProductReviewCountAsync(int productId)
        {
            return await _context.Reviews
                .CountAsync(r => r.ProductId == productId);
        }

        public async Task<int> GetServiceReviewCountAsync(int serviceId)
        {
            return await _context.Reviews
                .CountAsync(r => r.ServiceId == serviceId);
        }

        public async Task<Dictionary<int, int>> GetProductRatingDistributionAsync(int productId)
        {
            var distribution = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                result[i] = distribution.FirstOrDefault(d => d.Rating == i)?.Count ?? 0;
            }
            return result;
        }

        public async Task<Dictionary<int, int>> GetServiceRatingDistributionAsync(int serviceId)
        {
            var distribution = await _context.Reviews
                .Where(r => r.ServiceId == serviceId)
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                result[i] = distribution.FirstOrDefault(d => d.Rating == i)?.Count ?? 0;
            }
            return result;
        }
    }
} 