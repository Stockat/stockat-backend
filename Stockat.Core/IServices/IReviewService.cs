using Stockat.Core.DTOs.ReviewDTOs;
using Stockat.Core.Shared;
using Stockat.Core.Consts;
using Stockat.Core.DTOs;

namespace Stockat.Core.IServices
{
    public interface IReviewService
    {
        Task<GenericResponseDto<ReviewDto>> CreateReviewAsync(CreateReviewDto createReviewDto, string userId);
        Task<GenericResponseDto<ReviewDto>> UpdateReviewAsync(int reviewId, UpdateReviewDto updateReviewDto, string userId);
        Task<GenericResponseDto<bool>> DeleteReviewAsync(int reviewId, string userId);
        Task<GenericResponseDto<ReviewDto>> GetReviewByIdAsync(int reviewId);
        Task<GenericResponseDto<IEnumerable<ReviewDto>>> GetProductReviewsAsync(int productId, int page = 1, int size = 10);
        Task<GenericResponseDto<IEnumerable<ReviewDto>>> GetServiceReviewsAsync(int serviceId, int page = 1, int size = 10);
        Task<GenericResponseDto<IEnumerable<ReviewDto>>> GetUserReviewsAsync(string userId, int page = 1, int size = 10);
        Task<GenericResponseDto<ProductReviewSummaryDto>> GetProductReviewSummaryAsync(int productId);
        Task<GenericResponseDto<ServiceReviewSummaryDto>> GetServiceReviewSummaryAsync(int serviceId);
        Task<GenericResponseDto<bool>> CanUserReviewProductAsync(int orderProductId, string userId);
        Task<GenericResponseDto<bool>> CanUserReviewServiceAsync(int serviceRequestId, string userId);
        Task<GenericResponseDto<bool>> HasUserReviewedProductAsync(int orderProductId, string userId);
        Task<GenericResponseDto<bool>> HasUserReviewedServiceAsync(int serviceRequestId, string userId);
    }
} 