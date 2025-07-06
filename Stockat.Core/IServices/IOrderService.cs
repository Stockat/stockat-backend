using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.OrderDTOs;
using Stockat.Core.Enums;

namespace Stockat.Core.IServices
{
    public interface IOrderService
    {
        public Task<GenericResponseDto<AddOrderDTO>> AddOrderAsync(AddOrderDTO orderDto);
        public Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllSellerOrdersAsync();
        public Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllSellerRequestOrdersAsync();
        public Task<GenericResponseDto<OrderDTO>> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        public Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllOrdersandRequestforAdminAsync();
    }
}
