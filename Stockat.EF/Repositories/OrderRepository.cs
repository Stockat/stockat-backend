using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Stockat.Core.Consts;
using Stockat.Core.DTOs.OrderDTOs.OrderAnalysisDto;
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

    public ReportDto CalculateMonthlyRevenueOrderVsStatus(OrderType? type, OrderStatus? status, ReportMetricType metric)
    {
        var filteredOrders = _context.OrderProduct
      .Where(o =>
            (!type.HasValue || o.OrderType == type.Value) &&
            (!status.HasValue || o.Status == status.Value)
        )
        .AsEnumerable()
        .GroupBy(o => o.CraetedAt.Month)
        .ToDictionary(
            g => g.Key,
            g => metric == ReportMetricType.Revenue
                ? g.Sum(o => o.Price * o.Quantity)
                : g.Count()
        );

        string[] labels = new[]
        {
        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    };

        decimal[] values = new decimal[12];
        for (int i = 1; i <= 12; i++)
        {
            values[i - 1] = filteredOrders.ContainsKey(i) ? (decimal)filteredOrders[i] : 0;
        }

        return new ReportDto
        {
            Labels = labels,
            Values = values
        };
    }
    public ReportDto CalculateWeeklyRevenueOrderVsStatus(OrderType? type, OrderStatus? status, ReportMetricType metric)
    {
        var filteredOrders = _context.OrderProduct
       .Where(o =>
            (!type.HasValue || o.OrderType == type.Value) &&
            (!status.HasValue || o.Status == status.Value)
        )
        .AsEnumerable()
        .GroupBy(o => o.CraetedAt.DayOfWeek)
        .ToDictionary(
            g => g.Key,
            g => metric == ReportMetricType.Revenue
                ? g.Sum(o => o.Price * o.Quantity)
                : g.Count()
        );

        var labels = Enum.GetValues(typeof(DayOfWeek))
            .Cast<DayOfWeek>()
            .Select(d => d.ToString())
            .ToArray();

        var values = new decimal[7];
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            values[(int)day] = filteredOrders.ContainsKey(day)
                ? (decimal)filteredOrders[day]
                : 0;
        }

        return new ReportDto
        {
            Labels = labels,
            Values = values
        };
    }
    public ReportDto CalculateYearlyRevenueOrderVsStatus(OrderType? type, OrderStatus? status, ReportMetricType metric)
    {
        var filteredOrders = _context.OrderProduct.Where(o =>
            (!type.HasValue || o.OrderType == type.Value) &&
            (!status.HasValue || o.Status == status.Value)
        )
        .AsEnumerable()
        .GroupBy(o => o.CraetedAt.Year)
        .OrderBy(g => g.Key)
        .Select(g => new
        {
            Label = g.Key.ToString(),
            Value = metric == ReportMetricType.Revenue
                ? g.Sum(o => o.Price * o.Quantity)
                : g.Count()
        })
        .ToList();

        return new ReportDto
        {
            Labels = filteredOrders.Select(x => x.Label).ToArray(),
            Values = filteredOrders.Select(x => (decimal)x.Value).ToArray()
        };
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


    public TopProductReportDto GetTopProductPerYearAsync(OrderType? type, OrderStatus? status, ReportMetricType metric)
    {
        var orders = _context.OrderProduct
        .Include(o => o.Product)
        .Where(o =>
            (!type.HasValue || o.OrderType == type.Value) &&
            (!status.HasValue || o.Status == status.Value))
        .ToList();

        var grouped = orders
            .GroupBy(o => o.CraetedAt.Year)
            .OrderBy(g => g.Key)
            .Select(yearGroup =>
            {
                var top = yearGroup
                    .GroupBy(o => o.Product.Name)
                    .Select(g => new
                    {
                        Name = g.Key,
                        Value = metric == ReportMetricType.Revenue
                            ? g.Sum(o => o.Price * o.Quantity)
                            : g.Count()
                    })
                    .OrderByDescending(x => x.Value)
                    .FirstOrDefault();

                return new
                {
                    Label = yearGroup.Key.ToString(),
                    TopProduct = top?.Name ?? "N/A",
                    Value = top?.Value ?? 0
                };
            })
            .ToList();

        return new TopProductReportDto
        {
            Labels = grouped.Select(x => x.Label).ToArray(),
            TopProductNames = grouped.Select(x => x.TopProduct).ToArray(),
            TopProductValues = grouped.Select(x => (decimal)x.Value).ToArray()
        };
    }

    public TopProductReportDto GetTopProductPerMonthAsync(OrderType? type, OrderStatus? status, ReportMetricType metric)
    {
        var orders = _context.OrderProduct
      .Include(o => o.Product)
        .Where(o =>
            (!type.HasValue || o.OrderType == type.Value) &&
            (!status.HasValue || o.Status == status.Value))
        .ToList();

        var grouped = orders
            .GroupBy(o => o.CraetedAt.Month)
            .OrderBy(g => g.Key)
            .Select(monthGroup =>
            {
                var top = monthGroup
                    .GroupBy(o => o.Product.Name)
                    .Select(g => new
                    {
                        Name = g.Key,
                        Value = metric == ReportMetricType.Revenue
                            ? g.Sum(o => o.Price * o.Quantity)
                            : g.Count()
                    })
                    .OrderByDescending(x => x.Value)
                    .FirstOrDefault();

                return new
                {
                    Label = new DateTime(1, monthGroup.Key, 1).ToString("MMM"),
                    TopProduct = top?.Name ?? "N/A",
                    Value = top?.Value ?? 0
                };
            })
            .ToList();

        return new TopProductReportDto
        {
            Labels = grouped.Select(x => x.Label).ToArray(),
            TopProductNames = grouped.Select(x => x.TopProduct).ToArray(),
            TopProductValues = grouped.Select(x => (decimal)x.Value).ToArray()
        };
    }

    public TopProductReportDto GetTopProductPerWeekAsync(OrderType? type, OrderStatus? status, ReportMetricType metric)
    {
        var orders = _context.OrderProduct
        .Include(o => o.Product)
       .Include(o => o.Product)
        .Where(o =>
            (!type.HasValue || o.OrderType == type.Value) &&
            (!status.HasValue || o.Status == status.Value))
        .ToList();

        var grouped = orders
            .GroupBy(o => o.CraetedAt.DayOfWeek)
            .OrderBy(g => g.Key)
            .Select(dayGroup =>
            {
                var top = dayGroup
                    .GroupBy(o => o.Product.Name)
                    .Select(g => new
                    {
                        Name = g.Key,
                        Value = metric == ReportMetricType.Revenue
                            ? g.Sum(o => o.Price * o.Quantity)
                            : g.Count()
                    })
                    .OrderByDescending(x => x.Value)
                    .FirstOrDefault();

                return new
                {
                    Label = dayGroup.Key.ToString(),
                    TopProduct = top?.Name ?? "N/A",
                    Value = top?.Value ?? 0
                };
            })
            .ToList();

        return new TopProductReportDto
        {
            Labels = grouped.Select(x => x.Label).ToArray(),
            TopProductNames = grouped.Select(x => x.TopProduct).ToArray(),
            TopProductValues = grouped.Select(x => (decimal)x.Value).ToArray()
        };
    }





}
