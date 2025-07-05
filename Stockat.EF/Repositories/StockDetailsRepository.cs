using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.Entities;
using Stockat.Core.IRepositories;

namespace Stockat.EF.Repositories
{
    public class StockDetailsRepository : BaseRepository<Stock>
    {
        protected StockatDBContext _context;
        public StockDetailsRepository(StockatDBContext context) : base(context)
        {
            _context = context;
        }
        
    }
}
