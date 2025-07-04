using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Stockat.Core.Enums;

namespace Stockat.Core.Entities
{
    public class ServiceRequest
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Service ID is required.")]
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public Service Service { get; set; }

        [Required(ErrorMessage = "Request description is required.")]
        [MaxLength(1000, ErrorMessage = "Request description must not exceed 1000 characters.")]
        public string RequestDescription { get; set; }

        [Required(ErrorMessage = "Requested quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Requested quantity must be at least 1.")]
        public int RequestedQuantity { get; set; }

        [MaxLength(255)]
        public string? ImageId { get; set; }

        [Url(ErrorMessage = "Image URL must be a valid URL.")]
        [MaxLength(2083)] // Max length of a URL
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Buyer ID is required.")]
        public string BuyerId { get; set; }

        [ForeignKey("BuyerId")]
        public User Buyer { get; set; }

        [Required(ErrorMessage = "Price per product is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Price per product must be non-negative.")]
        public decimal PricePerProduct { get; set; }

        [MaxLength(100)]
        public string? EstimatedTime { get; set; }

        [Required]
        [MaxLength(20)]
        public ApprovalStatus SellerApprovalStatus { get; set; } = ApprovalStatus.Pending;

        [Required]
        [MaxLength(20)]
        public ApprovalStatus BuyerApprovalStatus { get; set; } = ApprovalStatus.Pending;

        public decimal TotalPrice { get; set; }

        [Required]
        [MaxLength(20)]
        public ServiceStatus ServiceStatus { get; set; } = ServiceStatus.Pending;

        [MaxLength(255)]
        public string? PaymentId { get; set; }

        [Required]
        [MaxLength(20)]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public int SellerOfferAttempts { get; set; } = 0;

        public ICollection<ServiceRequestUpdate> RequestUpdates { get; set; } = new List<ServiceRequestUpdate>();
    }
}
