using AutoMapper;
using Microsoft.AspNetCore.Http;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.ReviewDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;
using Stockat.Core.Shared;

namespace Stockat.Service.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReviewService(IRepositoryManager repositoryManager, IMapper mapper, ILoggerManager logger, IHttpContextAccessor httpContextAccessor)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<GenericResponseDto<ReviewDto>> CreateReviewAsync(CreateReviewDto createReviewDto, string userId)
        {
            try
            {
                // Validate that either product or service review
                if (!((createReviewDto.ProductId.HasValue && !createReviewDto.ServiceId.HasValue) || 
                      (!createReviewDto.ProductId.HasValue && createReviewDto.ServiceId.HasValue)))
                {
                    return new GenericResponseDto<ReviewDto>
                    {
                        Status = 400,
                        Message = "Review must be for either a product or a service, not both or neither."
                    };
                }

                // Check if user can review
                if (createReviewDto.ProductId.HasValue)
                {
                    if (!createReviewDto.OrderProductId.HasValue)
                    {
                        return new GenericResponseDto<ReviewDto>
                        {
                            Status = 400,
                            Message = "OrderProductId is required for product reviews."
                        };
                    }

                    var canReview = await CanUserReviewProductAsync(createReviewDto.OrderProductId.Value, userId);
                    if (canReview.Status != 200 || !canReview.Data)
                    {
                        return new GenericResponseDto<ReviewDto>
                        {
                            Status = 400,
                            Message = "You cannot review this product. Make sure the order is delivered and you haven't already reviewed it."
                        };
                    }
                }
                else
                {
                    if (!createReviewDto.ServiceRequestId.HasValue)
                    {
                        return new GenericResponseDto<ReviewDto>
                        {
                            Status = 400,
                            Message = "ServiceRequestId is required for service reviews."
                        };
                    }

                    var canReview = await CanUserReviewServiceAsync(createReviewDto.ServiceRequestId.Value, userId);
                    if (canReview.Status != 200 || !canReview.Data)
                    {
                        return new GenericResponseDto<ReviewDto>
                        {
                            Status = 400,
                            Message = "You cannot review this service. Make sure the service is delivered and you haven't already reviewed it."
                        };
                    }
                }

                var review = _mapper.Map<Review>(createReviewDto);
                review.ReviewerId = userId;

                _repositoryManager.ReviewRepo.Add(review);
                await _repositoryManager.CompleteAsync();

                var reviewDto = await GetReviewByIdAsync(review.Id);
                return reviewDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating review: {ex.Message}");
                return new GenericResponseDto<ReviewDto>
                {
                    Status = 500,
                    Message = "An error occurred while creating the review."
                };
            }
        }

        public async Task<GenericResponseDto<ReviewDto>> UpdateReviewAsync(int reviewId, UpdateReviewDto updateReviewDto, string userId)
        {
            try
            {
                var review = await _repositoryManager.ReviewRepo.GetByIdAsync(reviewId);
                if (review == null)
                {
                    return new GenericResponseDto<ReviewDto>
                    {
                        Status = 404,
                        Message = "Review not found."
                    };
                }

                if (review.ReviewerId != userId)
                {
                    return new GenericResponseDto<ReviewDto>
                    {
                        Status = 403,
                        Message = "You can only update your own reviews."
                    };
                }

                review.Rating = updateReviewDto.Rating;
                review.Comment = updateReviewDto.Comment;
                review.UpdatedAt = DateTime.UtcNow;

                _repositoryManager.ReviewRepo.Update(review);
                await _repositoryManager.CompleteAsync();

                var reviewDto = await GetReviewByIdAsync(reviewId);
                return reviewDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating review: {ex.Message}");
                return new GenericResponseDto<ReviewDto>
                {
                    Status = 500,
                    Message = "An error occurred while updating the review."
                };
            }
        }

        public async Task<GenericResponseDto<bool>> DeleteReviewAsync(int reviewId, string userId)
        {
            try
            {
                var review = await _repositoryManager.ReviewRepo.GetByIdAsync(reviewId);
                if (review == null)
                {
                    return new GenericResponseDto<bool>
                    {
                        Status = 404,
                        Message = "Review not found."
                    };
                }

                if (review.ReviewerId != userId)
                {
                    return new GenericResponseDto<bool>
                    {
                        Status = 403,
                        Message = "You can only delete your own reviews."
                    };
                }

                _repositoryManager.ReviewRepo.Delete(review);
                await _repositoryManager.CompleteAsync();

                return new GenericResponseDto<bool>
                {
                    Status = 200,
                    Message = "Review deleted successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting review: {ex.Message}");
                return new GenericResponseDto<bool>
                {
                    Status = 500,
                    Message = "An error occurred while deleting the review."
                };
            }
        }

        public async Task<GenericResponseDto<ReviewDto>> GetReviewByIdAsync(int reviewId)
        {
            try
            {
                var review = await _repositoryManager.ReviewRepo.FindAsync(r => r.Id == reviewId, includes: ["Reviewer", "Product", "Service", "OrderProduct", "ServiceRequest"]);
                if (review == null)
                {
                    return new GenericResponseDto<ReviewDto>
                    {
                        Status = 404,
                        Message = "Review not found."
                    };
                }

                var reviewDto = _mapper.Map<ReviewDto>(review);
                return new GenericResponseDto<ReviewDto>
                {
                    Status = 200,
                    Data = reviewDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting review: {ex.Message}");
                return new GenericResponseDto<ReviewDto>
                {
                    Status = 500,
                    Message = "An error occurred while retrieving the review."
                };
            }
        }

        public async Task<GenericResponseDto<IEnumerable<ReviewDto>>> GetProductReviewsAsync(int productId, int page = 1, int size = 10)
        {
            try
            {
                var skip = (page - 1) * size;
                var reviews = await _repositoryManager.ReviewRepo.GetProductReviewsAsync(productId, skip, size);
                var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);

                return new GenericResponseDto<IEnumerable<ReviewDto>>
                {
                    Status = 200,
                    Data = reviewDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting product reviews: {ex.Message}");
                return new GenericResponseDto<IEnumerable<ReviewDto>>
                {
                    Status = 500,
                    Message = "An error occurred while retrieving product reviews."
                };
            }
        }

        public async Task<GenericResponseDto<IEnumerable<ReviewDto>>> GetServiceReviewsAsync(int serviceId, int page = 1, int size = 10)
        {
            try
            {
                var skip = (page - 1) * size;
                var reviews = await _repositoryManager.ReviewRepo.GetServiceReviewsAsync(serviceId, skip, size);
                var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);

                return new GenericResponseDto<IEnumerable<ReviewDto>>
                {
                    Status = 200,
                    Data = reviewDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting service reviews: {ex.Message}");
                return new GenericResponseDto<IEnumerable<ReviewDto>>
                {
                    Status = 500,
                    Message = "An error occurred while retrieving service reviews."
                };
            }
        }

        public async Task<GenericResponseDto<IEnumerable<ReviewDto>>> GetUserReviewsAsync(string userId, int page = 1, int size = 10)
        {
            try
            {
                var skip = (page - 1) * size;
                var reviews = await _repositoryManager.ReviewRepo.GetUserReviewsAsync(userId, skip, size);
                var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);

                return new GenericResponseDto<IEnumerable<ReviewDto>>
                {
                    Status = 200,
                    Data = reviewDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user reviews: {ex.Message}");
                return new GenericResponseDto<IEnumerable<ReviewDto>>
                {
                    Status = 500,
                    Message = "An error occurred while retrieving user reviews."
                };
            }
        }

        public async Task<GenericResponseDto<ProductReviewSummaryDto>> GetProductReviewSummaryAsync(int productId)
        {
            try
            {
                var averageRating = await _repositoryManager.ReviewRepo.GetProductAverageRatingAsync(productId);
                var totalReviews = await _repositoryManager.ReviewRepo.GetProductReviewCountAsync(productId);
                var distribution = await _repositoryManager.ReviewRepo.GetProductRatingDistributionAsync(productId);

                var product = await _repositoryManager.ProductRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    return new GenericResponseDto<ProductReviewSummaryDto>
                    {
                        Status = 404,
                        Message = "Product not found."
                    };
                }

                var summary = new ProductReviewSummaryDto
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    AverageRating = averageRating,
                    TotalReviews = totalReviews,
                    FiveStarCount = distribution.GetValueOrDefault(5, 0),
                    FourStarCount = distribution.GetValueOrDefault(4, 0),
                    ThreeStarCount = distribution.GetValueOrDefault(3, 0),
                    TwoStarCount = distribution.GetValueOrDefault(2, 0),
                    OneStarCount = distribution.GetValueOrDefault(1, 0)
                };

                return new GenericResponseDto<ProductReviewSummaryDto>
                {
                    Status = 200,
                    Data = summary
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting product review summary: {ex.Message}");
                return new GenericResponseDto<ProductReviewSummaryDto>
                {
                    Status = 500,
                    Message = "An error occurred while retrieving product review summary."
                };
            }
        }

        public async Task<GenericResponseDto<ServiceReviewSummaryDto>> GetServiceReviewSummaryAsync(int serviceId)
        {
            try
            {
                var averageRating = await _repositoryManager.ReviewRepo.GetServiceAverageRatingAsync(serviceId);
                var totalReviews = await _repositoryManager.ReviewRepo.GetServiceReviewCountAsync(serviceId);
                var distribution = await _repositoryManager.ReviewRepo.GetServiceRatingDistributionAsync(serviceId);

                var service = await _repositoryManager.ServiceRepo.GetByIdAsync(serviceId);
                if (service == null)
                {
                    return new GenericResponseDto<ServiceReviewSummaryDto>
                    {
                        Status = 404,
                        Message = "Service not found."
                    };
                }

                var summary = new ServiceReviewSummaryDto
                {
                    ServiceId = serviceId,
                    ServiceName = service.Name,
                    AverageRating = averageRating,
                    TotalReviews = totalReviews,
                    FiveStarCount = distribution.GetValueOrDefault(5, 0),
                    FourStarCount = distribution.GetValueOrDefault(4, 0),
                    ThreeStarCount = distribution.GetValueOrDefault(3, 0),
                    TwoStarCount = distribution.GetValueOrDefault(2, 0),
                    OneStarCount = distribution.GetValueOrDefault(1, 0)
                };

                return new GenericResponseDto<ServiceReviewSummaryDto>
                {
                    Status = 200,
                    Data = summary
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting service review summary: {ex.Message}");
                return new GenericResponseDto<ServiceReviewSummaryDto>
                {
                    Status = 500,
                    Message = "An error occurred while retrieving service review summary."
                };
            }
        }

        public async Task<GenericResponseDto<bool>> CanUserReviewProductAsync(int orderProductId, string userId)
        {
            try
            {
                var orderProduct = await _repositoryManager.OrderRepo.GetByIdAsync(orderProductId);
                if (orderProduct == null)
                {
                    return new GenericResponseDto<bool>
                    {
                        Status = 404,
                        Message = "Order not found.",
                        Data = false
                    };
                }

                // Check if user is the buyer
                if (orderProduct.BuyerId != userId)
                {
                    return new GenericResponseDto<bool>
                    {
                        Status = 403,
                        Message = "Only the buyer can review this product.",
                        Data = false
                    };
                }

                // Check if order is delivered
                if (orderProduct.Status != OrderStatus.Delivered)
                {
                    return new GenericResponseDto<bool>
                    {
                        Status = 400,
                        Message = "You can only review products after they are delivered.",
                        Data = false
                    };
                }

                // Check if user already reviewed
                var hasReviewed = await _repositoryManager.ReviewRepo.HasUserReviewedProductAsync(orderProductId, userId);
                if (hasReviewed)
                {
                    return new GenericResponseDto<bool>
                    {
                        Status = 400,
                        Message = "You have already reviewed this product.",
                        Data = false
                    };
                }

                return new GenericResponseDto<bool>
                {
                    Status = 200,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking if user can review product: {ex.Message}");
                return new GenericResponseDto<bool>
                {
                    Status = 500,
                    Message = "An error occurred while checking review eligibility.",
                    Data = false
                };
            }
        }

        public async Task<GenericResponseDto<bool>> CanUserReviewServiceAsync(int serviceRequestId, string userId)
        {
            try
            {
                var serviceRequest = await _repositoryManager.ServiceRequestRepo.GetByIdAsync(serviceRequestId);
                if (serviceRequest == null)
                {
                    return new GenericResponseDto<bool>
                    {
                        Status = 404,
                        Message = "Service request not found.",
                        Data = false
                    };
                }

                // Check if user is the buyer
                if (serviceRequest.BuyerId != userId)
                {
                    return new GenericResponseDto<bool>
                    {
                        Status = 403,
                        Message = "Only the buyer can review this service.",
                        Data = false
                    };
                }

                // Check if service is delivered
                if (serviceRequest.ServiceStatus != ServiceStatus.Delivered)
                {
                    return new GenericResponseDto<bool>
                    {
                        Status = 400,
                        Message = "You can only review services after they are delivered.",
                        Data = false
                    };
                }

                // Check if user already reviewed
                var hasReviewed = await _repositoryManager.ReviewRepo.HasUserReviewedServiceAsync(serviceRequestId, userId);
                if (hasReviewed)
                {
                    return new GenericResponseDto<bool>
                    {
                        Status = 400,
                        Message = "You have already reviewed this service.",
                        Data = false
                    };
                }

                return new GenericResponseDto<bool>
                {
                    Status = 200,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking if user can review service: {ex.Message}");
                return new GenericResponseDto<bool>
                {
                    Status = 500,
                    Message = "An error occurred while checking review eligibility.",
                    Data = false
                };
            }
        }

        public async Task<GenericResponseDto<bool>> HasUserReviewedProductAsync(int orderProductId, string userId)
        {
            try
            {
                var hasReviewed = await _repositoryManager.ReviewRepo.HasUserReviewedProductAsync(orderProductId, userId);
                return new GenericResponseDto<bool>
                {
                    Status = 200,
                    Data = hasReviewed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking if user reviewed product: {ex.Message}");
                return new GenericResponseDto<bool>
                {
                    Status = 500,
                    Message = "An error occurred while checking review status.",
                    Data = false
                };
            }
        }

        public async Task<GenericResponseDto<bool>> HasUserReviewedServiceAsync(int serviceRequestId, string userId)
        {
            try
            {
                var hasReviewed = await _repositoryManager.ReviewRepo.HasUserReviewedServiceAsync(serviceRequestId, userId);
                return new GenericResponseDto<bool>
                {
                    Status = 200,
                    Data = hasReviewed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking if user reviewed service: {ex.Message}");
                return new GenericResponseDto<bool>
                {
                    Status = 500,
                    Message = "An error occurred while checking review status.",
                    Data = false
                };
            }
        }
    }
} 