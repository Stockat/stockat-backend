using Stockat.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.AuctionDTOs
{
    public class AuctionOrderCreateDto
    {
        public int AuctionId { get; set; }
        public int WinningBidId { get; set; }
        // New fields for shipping/order info
        public string? ShippingAddress { get; set; }
        public string? RecipientName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Notes { get; set; }
    }

    public class AuctionOrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public string PaymentTransactionId { get; set; }
        public bool? PaymentStatus { get; set; }
        public int AuctionId { get; set; }
        public int WinningBidId { get; set; }
        public decimal AmountPaid { get; set; }
        // New fields for shipping/order info
        public string? ShippingAddress { get; set; }
        public string? RecipientName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Notes { get; set; }
    }

    public class ProcessPaymentDto
    {
        public string PaymentTransactionId { get; set; }
        public bool PaymentSuccess { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public OrderStatus Status { get; set; }
    }

}
