using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using Stockat.Core.IRepositories;

namespace Stockat.EF.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    private readonly StockatDBContext _context;

    public UserRepository(StockatDBContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetTopSellersAsync(int limit = 5)
    {
        try
        {
            // Get users who have products and order by product count
            var topSellers = await _context.Users
                .Where(u => !u.IsDeleted)
                .Select(u => new
                {
                    User = u,
                    ProductCount = _context.Products.Count(p => p.SellerId == u.Id && !p.isDeleted)
                })
                .Where(x => x.ProductCount > 0)
                .OrderByDescending(x => x.ProductCount)
                .Take(20)
                .Select(x => x.User)
                .ToListAsync();

            return topSellers;
        }
        catch (Exception)
        {
            // Return empty list if there's an error
            return new List<User>();
        }
    }
} 