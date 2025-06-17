using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using Stockat.Core.IRepositories;

namespace Stockat.Core;

public interface IRepositoryManager
{
    IBaseRepository<UserVerification> UserVerificationRepo { get; }
    IProductRepository ProductRepository { get; }


    int Complete();
    void Dispose();


    Task<int> CompleteAsync();


    Task DisposeAsync();
}
