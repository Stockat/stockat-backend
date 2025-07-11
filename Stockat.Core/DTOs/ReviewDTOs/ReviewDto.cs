namespace Stockat.Core.DTOs.ReviewDTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public string ReviewerEmail { get; set; }
        public string? ReviewerImageUrl { get; set; }
        
        // For Product Reviews
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? OrderProductId { get; set; }
        
        // For Service Reviews
        public int? ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public int? ServiceRequestId { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 