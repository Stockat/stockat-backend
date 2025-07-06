using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.IServices.IAuctionServices
{
    public interface IAuctionOrderService
    {
        Task<AuctionOrderDto> CreateOrderForWinningBidAsync(int auctionId);
        Task<AuctionOrderDto> GetOrderByIdAsync(int id);
        Task<IEnumerable<AuctionOrderDto>> GetOrdersByUserAsync(string userId);
        public Task<AuctionOrderDto> GetOrderByAuctionIdAsync(int auctionId);
        Task ProcessPaymentAsync(int orderId, ProcessPaymentDto paymentDto);

        Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);

    }
}
