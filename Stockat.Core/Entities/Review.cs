using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Stockat.Core.Enums;

namespace Stockat.Core.Entities
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Review comment is required.")]
        [MinLength(10, ErrorMessage = "Review comment must be at least 10 characters long.")]
        [MaxLength(1000, ErrorMessage = "Review comment cannot exceed 1000 characters.")]
        public string Comment { get; set; }

        [Required(ErrorMessage = "Reviewer ID is required.")]
        public string ReviewerId { get; set; }

        [ForeignKey("ReviewerId")]
        public User Reviewer { get; set; }

        // For Product Reviews
        public int? ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        // For Service Reviews
        public int? ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public Service? Service { get; set; }

        // Reference to the order/service request that was reviewed
        public int? OrderProductId { get; set; }
        [ForeignKey("OrderProductId")]
        public OrderProduct? OrderProduct { get; set; }

        public int? ServiceRequestId { get; set; }
        [ForeignKey("ServiceRequestId")]
        public ServiceRequest? ServiceRequest { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Validation: Either ProductId or ServiceId must be provided, but not both
        public bool IsValid()
        {
            return (ProductId.HasValue && !ServiceId.HasValue) || (!ProductId.HasValue && ServiceId.HasValue);
        }
    }
} 