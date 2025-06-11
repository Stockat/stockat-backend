using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.AuctionDTOs
{
    public class AuctionBidRequestDto
    {
       public int Id { get; set; }
        public decimal BidAmount { get; set; }
        public string BidderId { get; set; }
        public int AuctionId   { get; set; }
    }
}
