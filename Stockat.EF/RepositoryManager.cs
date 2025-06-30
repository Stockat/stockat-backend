using AutoMapper;
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
    private readonly Lazy<ProductRepository> _productRepository;
    private readonly IMapper _mapper;

    private readonly Lazy<ServiceRepository> _serviceRepo;
    private readonly Lazy<IBaseRepository<ServiceRequest>> _serviceRequestRepo;
    private readonly Lazy<IBaseRepository<ServiceRequestUpdate>> _serviceRequestUpdateRepo;


    private readonly Lazy<IBaseRepository<User>> _userRepo;
    public RepositoryManager(StockatDBContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;

        _userVerificationRepo = new Lazy<IBaseRepository<UserVerification>>(() => new BaseRepository<UserVerification>(_context));
        _userRepo = new Lazy<IBaseRepository<User>>(()  => new BaseRepository<User>(_context));
        _productRepository = new Lazy<ProductRepository>(() => new ProductRepository(_context, _mapper));
        _serviceRepo = new Lazy<ServiceRepository>(() => new ServiceRepository(_context));
        _serviceRequestRepo = new Lazy<IBaseRepository<ServiceRequest>>(() => new BaseRepository<ServiceRequest>(_context));
        _serviceRequestUpdateRepo = new Lazy<IBaseRepository<ServiceRequestUpdate>>(() => new BaseRepository<ServiceRequestUpdate>(_context));
    }

    public IBaseRepository<UserVerification> UserVerificationRepo => _userVerificationRepo.Value;

    public IServiceRepository ServiceRepo => _serviceRepo.Value;
    public IBaseRepository<ServiceRequest> ServiceRequestRepo => _serviceRequestRepo.Value;
    public IBaseRepository<ServiceRequestUpdate> ServiceRequestUpdateRepo => _serviceRequestUpdateRepo.Value;
    public IProductRepository ProductRepository => _productRepository.Value;

    public IBaseRepository<User> UserRepo => _userRepo.Value;

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
