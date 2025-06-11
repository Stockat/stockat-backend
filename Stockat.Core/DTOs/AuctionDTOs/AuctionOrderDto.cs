using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.AuctionDTOs
{
    public class AuctionOrderDto
    {
       public int Id { get; set; }
        public DateTime OrderDate  { get; set; }
        public string Status   { get; set; }
        public bool? PaymentStatus     { get; set; }
        public string? PaymentTransactionId { get; set; }
        public int AuctionId {  get; set; }
        public int AuctionRequestId { get; set; }
    }
}
