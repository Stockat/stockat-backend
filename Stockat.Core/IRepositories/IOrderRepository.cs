using Stockat.Core.DTOs;
using Stockat.Core.DTOs.OrderDTOs.OrderAnalysisDto;
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

    public Task<ReportDto> CalculateOrdersVsPaymentStatusAsync(OrderType? type, ReportMetricType metricType);
    public ReportDto CalculateYearlyRevenueOrderVsStatus(OrderType? type, OrderStatus? status, ReportMetricType metric);
    public ReportDto CalculateMonthlyRevenueOrderVsStatus(OrderType? type, OrderStatus? status, ReportMetricType metric);
    public ReportDto CalculateWeeklyRevenueOrderVsStatus(OrderType? type, OrderStatus? status, ReportMetricType metric);


    public TopProductReportDto GetTopProductPerYearAsync(OrderType? type, OrderStatus? status, ReportMetricType metric);
    public TopProductReportDto GetTopProductPerMonthAsync(OrderType? type, OrderStatus? status, ReportMetricType metric);
    public TopProductReportDto GetTopProductPerWeekAsync(OrderType? type, OrderStatus? status, ReportMetricType metric);


    public Task<Dictionary<string, int>> GetOrderStatusCountsAsync();

}
