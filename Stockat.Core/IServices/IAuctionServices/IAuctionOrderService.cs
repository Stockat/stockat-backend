using Stockat.Core.DTOs;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.Enums;
using Stripe.Checkout;
//using Stripe.BillingPortal;
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

        //Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);

        // New: Mark payment as failed
        Task MarkPaymentFailedAsync(int orderId, string reason = null);

        // New: Update address/order info fields
        Task UpdateOrderAddressInfoAsync(int orderId, string shippingAddress, string recipientName, string phoneNumber, string notes);
        Task<IEnumerable<AuctionOrderDto>> GetAllOrdersAsync();

        //strip Payment
        public  Task HandleAuctionOrderFailure(Session session, string orderId);
        public Task HandleAuctionOrderCompletion(Session session, string orderId);

        public  Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
        public  Task UpdateOrderPaymentStatus(int orderId, PaymentStatus status) ;

        // New: Create Stripe checkout session for auction order
        Task<GenericResponseDto<AuctionOrderDto>> CreateStripeCheckoutSessionAsync(int orderId, string buyerId);
        
        // New: Update Stripe payment ID
        Task UpdateStripePaymentID(int orderId, string sessionId, string paymentIntentId);
    }
}
