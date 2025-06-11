using Stockat.Core.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.EF.UnitOfWork.AuctionUnitOfWork
{
    public class AuctionUnitOfWork
    {
        private readonly StockatDBContext _context;

        public IAuctionRepository Auctions { get; }


        public AuctionUnitOfWork(StockatDBContext context,
                          IAuctionRepository auctions)
        {
            _context = context;
            Auctions = auctions;
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
