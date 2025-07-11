using System.ComponentModel.DataAnnotations;

namespace Stockat.Core.DTOs.ReviewDTOs
{
    public class CreateReviewDto
    {
        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Review comment is required.")]
        [MinLength(10, ErrorMessage = "Review comment must be at least 10 characters long.")]
        [MaxLength(1000, ErrorMessage = "Review comment cannot exceed 1000 characters.")]
        public string Comment { get; set; }

        // For Product Reviews
        public int? ProductId { get; set; }
        public int? OrderProductId { get; set; }

        // For Service Reviews
        public int? ServiceId { get; set; }
        public int? ServiceRequestId { get; set; }
    }
} 