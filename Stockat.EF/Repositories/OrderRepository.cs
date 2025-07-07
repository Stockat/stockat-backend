using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.EF.Repositories;

public class OrderRepository : BaseRepository<OrderProduct>, IOrderRepository
{
    protected StockatDBContext _context;
    protected IMapper _mapper;

    public OrderRepository(StockatDBContext context, IMapper mapper) : base(context)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Dictionary<OrderType, int>> GetOrderCountsByTypeAsync()
    {
        var result = await _context.OrderProduct
            .GroupBy(op => op.OrderType)
            .Select(group => new
            {
                OrderType = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(x => x.OrderType, x => x.Count);

        return result;
    }

    public async Task<Dictionary<OrderType, decimal>> GetTotalSalesByOrderTypeAsync()
    {
        var result = await _context.OrderProduct
            .Where(op => op.Status != OrderStatus.Cancelled) // Optional: exclude cancelled
            .GroupBy(op => op.OrderType)
            .Select(group => new
            {
                OrderType = group.Key,
                TotalSales = group.Sum(op => op.Price * op.Quantity)
            })
            .ToDictionaryAsync(x => x.OrderType, x => x.TotalSales);

        return result;
    }





}
