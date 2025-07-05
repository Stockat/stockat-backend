using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using Stockat.Core.IRepositories;

namespace Stockat.EF.Repositories;

public class ServiceRepository : BaseRepository<Stockat.Core.Entities.Service>, IServiceRepository
{
    protected StockatDBContext _context;
    public ServiceRepository(StockatDBContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Service>> GetAllAvailableServicesWithSeller(int skip, int take)
    {
        return await _context.Set<Service>()
            .Skip(skip).Take(take)
            .Include(s => s.Seller)
            .ToListAsync();
    }

    public async Task<Service> GetByIdWithSeller(int id)
    {
        return await _context.Set<Service>()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<int> CountAllAvailableServicesAsync()
    {
        return await _context.Set<Service>().CountAsync();
    }
}
