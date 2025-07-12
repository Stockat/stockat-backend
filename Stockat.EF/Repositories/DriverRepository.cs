using AutoMapper;
using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.EF.Repositories
{
    public class DriverRepository : BaseRepository<Driver>
    {
        protected StockatDBContext _context;
        protected IMapper _mapper;

        public DriverRepository(StockatDBContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }


    }
}
