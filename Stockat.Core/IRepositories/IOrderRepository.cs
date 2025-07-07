using Stockat.Core.DTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.IRepositories;


public interface IOrderRepository : IBaseRepository<OrderProduct>
{
    public Task<Dictionary<OrderType, int>> GetOrderCountsByTypeAsync();
    public Task<Dictionary<OrderType, decimal>> GetTotalSalesByOrderTypeAsync();
}
