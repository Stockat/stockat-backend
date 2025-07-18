﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockat.API.ActionFilters;
using Stockat.Core;
using Stockat.Core.DTOs.ServiceRequestDTOs;
using Stockat.Core.Exceptions;

namespace Stockat.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServiceRequestController : ControllerBase
{
    IServiceManager _serviceManager;
    public ServiceRequestController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    [HttpPost]
    [Authorize]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> CreateServiceRequestAsync([FromBody] CreateServiceRequestDto dto)
    {
        try
        {
            var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(buyerId))
            {
                return Unauthorized("User is not authenticated.");
            }

            var result = await _serviceManager.ServiceRequestService.CreateAsync(dto, buyerId);
            return Created($"/api/servicerequest/{result.Id}", result);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("{requestId:int}")]
    [Authorize]
    public async Task<IActionResult> GetByIdAsync(int requestId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            var isSeller = User.IsInRole("Seller");


            var result = await _serviceManager.ServiceRequestService.GetByIdAsync(requestId, userId, isSeller);
            if (result == null)
            {
                return NotFound($"Service request with ID {requestId} not found.");
            }
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetBuyerRequestsAsync([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authenticated.");
        }

        var requests = await _serviceManager.ServiceRequestService.GetBuyerRequestsAsync(userId, page, size);
        return Ok(requests);
    }

    [HttpGet("{serviceId:int}/incoming")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerRequestsAsync(int serviceId, [FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authenticated.");
        }
        var requests = await _serviceManager.ServiceRequestService.GetSellerRequestsAsync(userId, serviceId, page, size);
        return Ok(requests);
    }

    [HttpPatch("{requestId:int}/status")]
    [Authorize]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> UpdateBuyerStatusAsync(int requestId, [FromBody] ApprovalStatusDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User is not authenticated.");
            
            var result = await _serviceManager.ServiceRequestService.UpdateBuyerStatusAsync(requestId, userId, dto);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPatch("{requestId:int}/seller-offer")]
    [Authorize(Roles = "Seller")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> SetSellerOfferAsync(int requestId, [FromBody] SellerOfferDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User is not authenticated.");

            var result = await _serviceManager.ServiceRequestService.SetSellerOfferAsync(requestId, userId, dto);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
        }
    }

    // update request status
    [HttpPatch("{requestId:int}/status/update")]
    [Authorize(Roles = "Seller, Admin")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> UpdateServiceStatusAsync(int requestId, [FromBody] ServiceStatusDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User is not authenticated.");

            bool isAdmin = User.IsInRole("Admin");
            var result = await _serviceManager.ServiceRequestService.UpdateServiceStatusAsync(requestId, userId, isAdmin, dto);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("buyer/pending-services")]
    [Authorize]
    public async Task<IActionResult> GetBuyerServiceIDsWithPendingRequests()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User is not authenticated.");
            var serviceIds = await _serviceManager.ServiceRequestService.GetBuyerServiceIDsWithPendingRequests(userId);
            return Ok(serviceIds);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPatch("{requestId:int}/buyer-cancel")]
    [Authorize]
    public async Task<IActionResult> CancelBuyerRequestAsync(int requestId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User is not authenticated.");
            var result = await _serviceManager.ServiceRequestService.CancelBuyerRequest(requestId, userId);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllRequestsForAdmin([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] int? status = null)
    {
        var statusEnum = status.HasValue ? (Stockat.Core.Enums.ServiceStatus?)status.Value : null;
        var result = await _serviceManager.ServiceRequestService.GetAllRequestsForAdminAsync(page, size, statusEnum);
        return Ok(result);
    }

    [HttpPost("{requestId:int}/checkout")]
    [Authorize]
    public async Task<IActionResult> CreateCheckoutSession(int requestId)
    {
        try
        {
            var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(buyerId))
                return Unauthorized("User is not authenticated.");

            var result = await _serviceManager.ServiceRequestService.CreateStripeCheckoutSessionAsync(requestId, buyerId);
            return StatusCode(result.Status, result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
        }
    }

    // --- SELLER ANALYTICS ENDPOINTS ---
    [HttpGet("seller/analytics/status-breakdown")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerServiceRequestStatusBreakdown()
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("User is not authenticated.");
        var result = await _serviceManager.ServiceRequestService.GetSellerServiceRequestStatusBreakdownAsync(sellerId);
        return Ok(result);
    }

    [HttpGet("seller/analytics/monthly-trend")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerServiceRequestMonthlyTrend()
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("User is not authenticated.");
        var result = await _serviceManager.ServiceRequestService.GetSellerServiceRequestMonthlyTrendAsync(sellerId);
        return Ok(result);
    }

    [HttpGet("seller/analytics/revenue")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerServiceRevenue()
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("User is not authenticated.");
        var result = await _serviceManager.ServiceRequestService.GetSellerServiceRevenueAsync(sellerId);
        return Ok(result);
    }

    [HttpGet("seller/analytics/top-services")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerTopServicesByRequests()
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("User is not authenticated.");
        var result = await _serviceManager.ServiceRequestService.GetSellerTopServicesByRequestsAsync(sellerId);
        return Ok(result);
    }

    [HttpGet("seller/analytics/customer-feedback")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerCustomerFeedback()
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("User is not authenticated.");
        var result = await _serviceManager.ServiceRequestService.GetSellerCustomerFeedbackAsync(sellerId);
        return Ok(result);
    }

    [HttpGet("seller/analytics/conversion-funnel")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerConversionFunnel()
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("User is not authenticated.");
        var result = await _serviceManager.ServiceRequestService.GetSellerConversionFunnelAsync(sellerId);
        return Ok(result);
    }

    [HttpGet("seller/analytics/service-reviews")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerServiceReviews()
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("User is not authenticated.");
        var result = await _serviceManager.ServiceRequestService.GetSellerServiceReviewsAsync(sellerId);
        return Ok(result);
    }

    [HttpGet("seller/analytics/top-customers")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerTopCustomers()
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("User is not authenticated.");
        var result = await _serviceManager.ServiceRequestService.GetSellerTopCustomersAsync(sellerId);
        return Ok(result);
    }

    [HttpGet("seller/analytics/customer-demographics")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerCustomerDemographics()
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("User is not authenticated.");
        var result = await _serviceManager.ServiceRequestService.GetSellerCustomerDemographicsAsync(sellerId);
        return Ok(result);
    }
}
