﻿using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.IRepositories
{
    public interface IAuctionRepository: IBaseRepository<Auction>
    {
        public Task<AuctionOrder> GetByIdAsync(int id, string[] includes = null);
    }
}
