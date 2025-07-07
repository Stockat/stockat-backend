using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stockat.Core.Entities
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Service name is required.")]
        [MaxLength(100, ErrorMessage = "Service name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Service description is required.")]
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Minimum Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Minimum quantity must be at least 1.")]
        public int MinQuantity { get; set; } = 1;

        [Required(ErrorMessage = "Price per product is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")] // 0.01 for free or near-zero services
        public decimal PricePerProduct { get; set; }

        [Required(ErrorMessage = "Estimated time is required.")]
        [MaxLength(50, ErrorMessage = "Estimated time cannot exceed 50 characters.")]
        public string EstimatedTime { get; set; }

        [MaxLength(255)]
        [Required(ErrorMessage = "Image ID is required.")]
        public string ImageId { get; set; }

        [Url(ErrorMessage = "Image URL must be a valid URL.")]
        [MaxLength(2083)]
        [Required(ErrorMessage = "Image URL is required.")]
        public string ImageUrl { get; set; }

        [Required]
        public bool IsApproved { get; set; } = false;

        [Required(ErrorMessage = "Seller ID is required.")]
        public string SellerId { get; set; }

        [ForeignKey("SellerId")]
        public User Seller { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}
