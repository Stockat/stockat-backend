﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.Enums;

namespace Stockat.Core.DTOs.OrderDTOs
{
    public class AddOrderDTO
    {
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public OrderType OrderType { get; set; } = OrderType.Order; // Default to Order, can be Request
        public OrderStatus Status { get; set; } = OrderStatus.Pending; // Default to Pending
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int StockId { get; set; }
        public string SellerId { get; set; } // Seller's UserId
        public string BuyerId { get; set; } // Buyer's UserId
        public string PaymentId { get; set; } // Payment Gateway Transaction Id
        public string PaymentStatus { get; set; } // Status of the payment (e.g., Completed, Failed)
    }
}
