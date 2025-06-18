using Stockat.Core.Entities;
using Stockat.Core.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.EF.Repositories
{
    public class AuctionBidRequestRepository : BaseRepository<AuctionBidRequest>, IAuctionBidRequestReopsitory
    {
        public AuctionBidRequestRepository(StockatDBContext context) : base(context)
        {
            _context = context;
        }
    }
}
