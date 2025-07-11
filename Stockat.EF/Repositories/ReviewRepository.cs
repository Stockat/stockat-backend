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
            try
            {
                return await _context.Reviews
                    .Where(r => r.ProductId == productId)
                    .Include(r => r.Reviewer)
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Review>();
            }
        }

        public async Task<IEnumerable<Review>> GetServiceReviewsAsync(int serviceId, int skip, int take)
        {
            try
            {
                return await _context.Reviews
                    .Where(r => r.ServiceId == serviceId)
                    .Include(r => r.Reviewer)
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Review>();
            }
        }

        public async Task<IEnumerable<Review>> GetUserReviewsAsync(string userId, int skip, int take)
        {
            try
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
            catch (Exception)
            {
                return new List<Review>();
            }
        }

        public async Task<Review?> GetReviewByOrderProductAsync(int orderProductId, string userId)
        {
            try
            {
                return await _context.Reviews
                    .FirstOrDefaultAsync(r => r.OrderProductId == orderProductId && r.ReviewerId == userId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Review?> GetReviewByServiceRequestAsync(int serviceRequestId, string userId)
        {
            try
            {
                return await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ServiceRequestId == serviceRequestId && r.ReviewerId == userId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> HasUserReviewedProductAsync(int orderProductId, string userId)
        {
            try
            {
                return await _context.Reviews
                    .AnyAsync(r => r.OrderProductId == orderProductId && r.ReviewerId == userId);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> HasUserReviewedServiceAsync(int serviceRequestId, string userId)
        {
            try
            {
                return await _context.Reviews
                    .AnyAsync(r => r.ServiceRequestId == serviceRequestId && r.ReviewerId == userId);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<double> GetProductAverageRatingAsync(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            if (!reviews.Any())
                return 0.0;

            var average = reviews.Average(r => (double)r.Rating);
            return Math.Round(average, 1);
        }

        public async Task<double> GetServiceAverageRatingAsync(int serviceId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ServiceId == serviceId)
                .ToListAsync();

            if (!reviews.Any())
                return 0.0;

            var average = reviews.Average(r => (double)r.Rating);
            return Math.Round(average, 1);
        }

        public async Task<int> GetProductReviewCountAsync(int productId)
        {
            try
            {
                return await _context.Reviews
                    .CountAsync(r => r.ProductId == productId);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<int> GetServiceReviewCountAsync(int serviceId)
        {
            try
            {
                return await _context.Reviews
                    .CountAsync(r => r.ServiceId == serviceId);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<Dictionary<int, int>> GetProductRatingDistributionAsync(int productId)
        {
            try
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
            catch (Exception)
            {
                // Return empty distribution if there's an error
                return new Dictionary<int, int>
                {
                    { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
                };
            }
        }

        public async Task<Dictionary<int, int>> GetServiceRatingDistributionAsync(int serviceId)
        {
            try
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
            catch (Exception)
            {
                // Return empty distribution if there's an error
                return new Dictionary<int, int>
                {
                    { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
                };
            }
        }
    }
} 