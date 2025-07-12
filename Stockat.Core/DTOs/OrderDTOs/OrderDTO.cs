using System;
using Stockat.Core.Enums;

namespace Stockat.Core.DTOs.OrderDTOs
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public OrderType OrderType { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CraetedAt { get; set; }
        public string? Description { get; set; } = string.Empty;
        public string? ProductName { get; set; } = string.Empty;

        // Payment Info
        public string PaymentId { get; set; }
        public string PaymentStatus { get; set; }

        // Product and Stock Info
        public int ProductId { get; set; }
        public int StockId { get; set; }

        // User Info
        public string SellerId { get; set; }
        public string SellerFirstName { get; set; }
        public string SellerLastName { get; set; }

        public string BuyerId { get; set; }
        public string BuyerFirstName { get; set; }
        public string BuyerLastName { get; set; }

        public string DriverId { get; set; }
    }
}
