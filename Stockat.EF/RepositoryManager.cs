using Stockat.Core;
using Stockat.Core.Entities;
using Stockat.Core.IRepositories;
using Stockat.Core.IServices;
using Stockat.EF.Repositories;

namespace Stockat.EF;

public class RepositoryManager : IRepositoryManager
{
    private readonly StockatDBContext _context;
    private readonly Lazy<IBaseRepository<UserVerification>> _userVerificationRepo;
    public RepositoryManager(StockatDBContext context)
    {
        _context = context;
        _userVerificationRepo = new Lazy<IBaseRepository<UserVerification>>(() => new BaseRepository<UserVerification>(_context));
    }

    public IBaseRepository<UserVerification> UserVerificationRepo => _userVerificationRepo.Value;




    public int Complete()
    {
        return _context.SaveChanges(); // returns no. of rows affected
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();   
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async Task DisposeAsync()
    {
        _context.DisposeAsync();
    }
}
