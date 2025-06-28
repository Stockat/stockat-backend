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
    private readonly Lazy<IBaseRepository<Service>> _serviceRepo;
    private readonly Lazy<IBaseRepository<ServiceRequest>> _serviceRequestRepo;
    private readonly Lazy<IBaseRepository<ServiceRequestUpdate>> _serviceRequestUpdateRepo;

    public RepositoryManager(StockatDBContext context)
    {
        _context = context;
        _userVerificationRepo = new Lazy<IBaseRepository<UserVerification>>(() => new BaseRepository<UserVerification>(_context));
        _serviceRepo = new Lazy<IBaseRepository<Service>>(() => new BaseRepository<Service>(_context));
        _serviceRequestRepo = new Lazy<IBaseRepository<ServiceRequest>>(() => new BaseRepository<ServiceRequest>(_context));
        _serviceRequestUpdateRepo = new Lazy<IBaseRepository<ServiceRequestUpdate>>(() => new BaseRepository<ServiceRequestUpdate>(_context));
    }

    public IBaseRepository<UserVerification> UserVerificationRepo => _userVerificationRepo.Value;

    public IBaseRepository<Service> ServiceRepo => _serviceRepo.Value;
    public IBaseRepository<ServiceRequest> ServiceRequestRepo => _serviceRequestRepo.Value;
    public IBaseRepository<ServiceRequestUpdate> ServiceRequestUpdateRepo => _serviceRequestUpdateRepo.Value;

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
