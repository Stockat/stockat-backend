using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using Stockat.Core.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.EF.Repositories
{
    public class AuctionRepository : BaseRepository<Auction> ,IAuctionRepository
    {

        public AuctionRepository(StockatDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<AuctionOrder> GetByIdAsync(int id, string[] includes = null)
        {
            IQueryable<AuctionOrder> query = _context.Set<AuctionOrder>();

            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
