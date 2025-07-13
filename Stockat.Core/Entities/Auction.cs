using Stockat.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.Entities
{
    public class Auction
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal StartingPrice { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBid { get; set; }

        public int Quantity { get; set; }

        public bool IsClosed { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal IncrementUnit { get; set; }

        public bool IsDeleted { get; set; } = false;

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int StockId { get; set; }

        [ForeignKey(nameof(StockId))]
        public Stock Stock { get; set; }

        public string? BuyerId { get; set; }
        [ForeignKey(nameof(BuyerId))]
        public User? BuyerUser { get; set; }

        [Required]
        public string SellerId { get; set; }
        public User SellerUser { get; set; }

        public ICollection<AuctionBidRequest> AuctionBidRequest { get; set; } = new List<AuctionBidRequest>();

        public AuctionOrder? AuctionOrder { get; set; }


    }

    public class AuctionBidRequest
    {
        [Key]
        public int Id { get; set; }

        public int AuctionId { get; set; }
        public Auction Auction { get; set; }

        public string BidderId { get; set; }
        [ForeignKey(nameof(BidderId))]
        public User BidderUser { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BidAmount { get; set; }
        public AuctionOrder? AuctionOrder { get; set; }
    }



    public class AuctionOrder
    {
        [Key]
        public int Id { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public OrderStatus? Status { get; set; } = OrderStatus.Pending;

        public string? PaymentTransactionId { get; set; }

       // public bool? PaymentStatus { get; set; }

        public string? StripeSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public int AuctionId { get; set; }
        public Auction Auction { get; set; }

        public int AuctionRequestId { get; set; }
        public AuctionBidRequest AuctionRequest { get; set; }

        // New fields for shipping/order info
        public string? ShippingAddress { get; set; }
        public string? RecipientName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Notes { get; set; }
    }
}
