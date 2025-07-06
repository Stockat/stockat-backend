using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.AuctionDTOs
{
    public class AuctionDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal CurrentBid { get; set; }
        public decimal IncrementUnit { get; set; }
        public int Quantity { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsClosed { get; set; }

        public string? BuyerId { get; set; }
        public string SellerId { get; set; }
        public int ProductId { get; set; }
        public int StockId { get; set; }
    }
}
