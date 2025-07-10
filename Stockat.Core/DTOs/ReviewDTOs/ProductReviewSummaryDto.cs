namespace Stockat.Core.DTOs.ReviewDTOs
{
    public class ProductReviewSummaryDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
    }
} 