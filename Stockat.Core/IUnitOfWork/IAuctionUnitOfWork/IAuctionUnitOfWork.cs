using Stockat.Core.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.IUnitOfWork.IAuctionUnitOfWork
{
    public interface IAuctionUnitOfWork: IDisposable
    {
        IAuctionRepository Auctions { get; }
        //product and user repos should be here

        Task<int> CompleteAsync();
    }
}
