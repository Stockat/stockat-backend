using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.ReviewDTOs;
using Stockat.Core.Shared;
using System.Security.Claims;

namespace Stockat.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public ReviewController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpPost]
        public async Task<ActionResult<GenericResponseDto<ReviewDto>>> CreateReview([FromBody] CreateReviewDto createReviewDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new GenericResponseDto<ReviewDto>
                {
                    Status = 401,
                    Message = "User not authenticated."
                });
            }

            var result = await _serviceManager.ReviewService.CreateReviewAsync(createReviewDto, userId);
            
            if (result.Status != 200)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetReviewById), new { id = result.Data.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<GenericResponseDto<ReviewDto>>> UpdateReview(int id, [FromBody] UpdateReviewDto updateReviewDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new GenericResponseDto<ReviewDto>
                {
                    Status = 401,
                    Message = "User not authenticated."
                });
            }

            var result = await _serviceManager.ReviewService.UpdateReviewAsync(id, updateReviewDto, userId);
            
            if (result.Status != 200)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<GenericResponseDto<bool>>> DeleteReview(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new GenericResponseDto<bool>
                {
                    Status = 401,
                    Message = "User not authenticated."
                });
            }

            var result = await _serviceManager.ReviewService.DeleteReviewAsync(id, userId);
            
            if (result.Status != 200)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<GenericResponseDto<ReviewDto>>> GetReviewById(int id)
        {
            var result = await _serviceManager.ReviewService.GetReviewByIdAsync(id);
            
            if (result.Status != 200)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<ActionResult<GenericResponseDto<IEnumerable<ReviewDto>>>> GetProductReviews(
            int productId, 
            [FromQuery] int page = 1, 
            [FromQuery] int size = 10)
        {
            var result = await _serviceManager.ReviewService.GetProductReviewsAsync(productId, page, size);
            
            if (result.Status != 200)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("service/{serviceId}")]
        [AllowAnonymous]
        public async Task<ActionResult<GenericResponseDto<IEnumerable<ReviewDto>>>> GetServiceReviews(
            int serviceId, 
            [FromQuery] int page = 1, 
            [FromQuery] int size = 10)
        {
            var result = await _serviceManager.ReviewService.GetServiceReviewsAsync(serviceId, page, size);
            
            if (result.Status != 200)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("user")]
        public async Task<ActionResult<GenericResponseDto<IEnumerable<ReviewDto>>>> GetUserReviews(
            [FromQuery] int page = 1, 
            [FromQuery] int size = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new GenericResponseDto<IEnumerable<ReviewDto>>
                {
                    Status = 401,
                    Message = "User not authenticated."
                });
            }

            var result = await _serviceManager.ReviewService.GetUserReviewsAsync(userId, page, size);
            
            if (result.Status != 200)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("product/{productId}/summary")]
        [AllowAnonymous]
        public async Task<ActionResult<GenericResponseDto<ProductReviewSummaryDto>>> GetProductReviewSummary(int productId)
        {
            var result = await _serviceManager.ReviewService.GetProductReviewSummaryAsync(productId);
            
            if (result.Status != 200)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("service/{serviceId}/summary")]
        [AllowAnonymous]
        public async Task<ActionResult<GenericResponseDto<ServiceReviewSummaryDto>>> GetServiceReviewSummary(int serviceId)
        {
            var result = await _serviceManager.ReviewService.GetServiceReviewSummaryAsync(serviceId);
            
            if (result.Status != 200)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("can-review-product/{orderProductId}")]
        public async Task<ActionResult<GenericResponseDto<bool>>> CanReviewProduct(int orderProductId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new GenericResponseDto<bool>
                {
                    Status = 401,
                    Message = "User not authenticated."
                });
            }

            var result = await _serviceManager.ReviewService.CanUserReviewProductAsync(orderProductId, userId);
            
            if (result.Status != 200)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("can-review-service/{serviceRequestId}")]
        public async Task<ActionResult<GenericResponseDto<bool>>> CanReviewService(int serviceRequestId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new GenericResponseDto<bool>
                {
                    Status = 401,
                    Message = "User not authenticated."
                });
            }

            var result = await _serviceManager.ReviewService.CanUserReviewServiceAsync(serviceRequestId, userId);
            
            if (result.Status != 200)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("has-reviewed-product/{orderProductId}")]
        public async Task<ActionResult<GenericResponseDto<bool>>> HasReviewedProduct(int orderProductId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new GenericResponseDto<bool>
                {
                    Status = 401,
                    Message = "User not authenticated."
                });
            }

            var result = await _serviceManager.ReviewService.HasUserReviewedProductAsync(orderProductId, userId);
            
            if (result.Status != 200)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("has-reviewed-service/{serviceRequestId}")]
        public async Task<ActionResult<GenericResponseDto<bool>>> HasReviewedService(int serviceRequestId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new GenericResponseDto<bool>
                {
                    Status = 401,
                    Message = "User not authenticated."
                });
            }

            var result = await _serviceManager.ReviewService.HasUserReviewedServiceAsync(serviceRequestId, userId);
            
            if (result.Status != 200)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
} 