using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Stockat.Core.Enums;

namespace Stockat.Core.Entities
{
    public class ServiceRequestUpdate
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Service Request ID is required.")]
        public int ServiceRequestId { get; set; }

        [Required(ErrorMessage = "Total old price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Total old price must be non-negative.")]
        public decimal TotalOldPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Additional price must be non-negative.")]
        public decimal AdditionalPrice { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Additional quantity must be non-negative.")]
        public int AdditionalQuantity { get; set; } = 0;

        [MaxLength(100, ErrorMessage = "Additional time must be at most 100 characters.")]
        public string? AdditionalTime { get; set; } 

        [Required]
        [MaxLength(20)]
        public ServiceStatus Status { get; set; } = ServiceStatus.Pending;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigations
        [ForeignKey("ServiceRequestId")]
        public ServiceRequest ServiceRequest { get; set; }
    }
}
