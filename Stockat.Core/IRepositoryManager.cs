using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using Stockat.Core.IRepositories;

namespace Stockat.Core;

public interface IRepositoryManager
{
    IBaseRepository<UserVerification> UserVerificationRepo { get; }
    IBaseRepository<Stock> StockRepo { get; }
    IBaseRepository<Auction> AuctionRepo { get; }
    IBaseRepository<AuctionBidRequest> AuctionBidRequestRepo { get; }
    IBaseRepository<AuctionOrder> AuctionOrderRepo { get; }



    int Complete();
    void Dispose();


    Task<int> CompleteAsync();


    Task DisposeAsync();

    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
