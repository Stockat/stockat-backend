using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.OrderDTOs;
using Stockat.Core.DTOs.OrderDTOs.OrderAnalysisDto;
using Stockat.Core.Entities;
using Stockat.Core.Enums;

namespace Stockat.Core.IServices
{
    public interface IOrderService
    {
        public Task<GenericResponseDto<AddOrderDTO>> AddOrderAsync(AddOrderDTO orderDto, string domain);
        public Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllSellerOrdersAsync();
        public Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllSellerRequestOrdersAsync();
        public Task<GenericResponseDto<OrderDTO>> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        public Task<GenericResponseDto<OrderDTO>> UpdateRequestOrderStatusAsync(UpdateReqDto updateReq);
        public Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllOrdersandRequestforAdminAsync();

        public Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllUserOrdersAsync();
        public Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllUserRequestOrdersAsync();

        public Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllBuyerOrdersAsync();
        public Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllBuyerRequestOrdersAsync();

        public Task<GenericResponseDto<AddRequestDTO>> AddRequestAsync(AddRequestDTO requestDto);
        // Analysis 
        public Task<GenericResponseDto<Dictionary<OrderType, int>>> GetOrderCountsByTypeAsync();
        public Task<GenericResponseDto<Dictionary<OrderType, decimal>>> GetTotalSalesByOrderTypeAsync();
        public Task<GenericResponseDto<ReportDto>> CalculateOrdersVsPaymentStatusAsync(OrderType? type, ReportMetricType metricType);
        public GenericResponseDto<ReportDto> CalculateMonthlyRevenueOrderVsStatus(OrderType? type, OrderStatus? status, ReportMetricType metricType);
        public GenericResponseDto<ReportDto> CalculateWeeklyRevenueOrderVsStatus(OrderType? type, OrderStatus? status, ReportMetricType metricType);
        public GenericResponseDto<ReportDto> CalculateYearlyRevenueOrderVsStatus(OrderType? type, OrderStatus? status, ReportMetricType metricType);

        public GenericResponseDto<TopProductReportDto> GetTopProductPerYearAsync(OrderType? type, OrderStatus? status, ReportMetricType metric);
        public GenericResponseDto<TopProductReportDto> GetTopProductPerMonthAsync(OrderType? type, OrderStatus? status, ReportMetricType metric);
        public GenericResponseDto<TopProductReportDto> GetTopProductPerWeekAsync(OrderType? type, OrderStatus? status, ReportMetricType metric);

        // Stripe Internals 
        public Task UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
        // Stripe With Req When Processing
        public Task<GenericResponseDto<UpdateRequestDTO>> AddStripeWithRequestAsync(UpdateRequestDTO requestDto);

        // Order Status Update
        public Task UpdateStatus(int id, OrderStatus orderStatus, PaymentStatus paymentStatus);
        public Task<GenericResponseDto<OrderDTO>> CancelOrderOnPaymentFailureAsync(string sessionId);



        // 
        public Task<OrderProduct> getOrderByIdAsync(int id);

        public Task InvoiceGeneratorAsync(int orderid);

        public Task PaymentCancellation();



        public Task<GenericResponseDto<Dictionary<string, int>>> OrderSummaryCalc();

        public Task<OrderProduct> getorderbySessionOrPaymentId(string id);


    }
}