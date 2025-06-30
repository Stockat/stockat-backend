using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using Stockat.Core.IRepositories;

namespace Stockat.Core;

public interface IRepositoryManager
{
    IBaseRepository<UserVerification> UserVerificationRepo { get; }
    IBaseRepository<Service> ServiceRepo { get; }
    IBaseRepository<ServiceRequest> ServiceRequestRepo { get; }
    IBaseRepository<ServiceRequestUpdate> ServiceRequestUpdateRepo { get; }
    IProductRepository ProductRepository { get; }
    IBaseRepository<Stock> StockRepo { get; }




    IBaseRepository<User> UserRepo { get; }
    int Complete();
    void Dispose();


    Task<int> CompleteAsync();


    Task DisposeAsync();
}
