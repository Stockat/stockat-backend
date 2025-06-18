using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.AuctionDTOs
{
    public class AuctionBidRequestCreateDto
    {
        public int AuctionId { get; set; }
        public string BidderId { get; set; }
        public decimal BidAmount { get; set; }
    }

    public class AuctionBidRequestDto
    {
        public int Id { get; set; }
        public int AuctionId { get; set; }
        public string BidderId { get; set; }
        public decimal BidAmount { get; set; }
        public int? AuctionOrderId { get; set; }
    }
}
