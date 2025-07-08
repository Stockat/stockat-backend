using Stockat.Core.Entities;

namespace Stockat.Core.IRepositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<IEnumerable<User>> GetTopSellersAsync(int limit = 5);
} 