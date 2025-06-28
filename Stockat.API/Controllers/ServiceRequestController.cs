using System.Security.Claims;
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
    public async Task<IActionResult> GetBuyerRequestsAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authenticated.");
        }

        var requests = await _serviceManager.ServiceRequestService.GetBuyerRequestsAsync(userId);
        return Ok(requests);
    }

    [HttpGet("{serviceId:int}/incoming")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerRequestsAsync(int serviceId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authenticated.");
        }
        var requests = await _serviceManager.ServiceRequestService.GetSellerRequestsAsync(userId, serviceId);
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
    [Authorize(Roles = "Seller")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> UpdateServiceStatusAsync(int requestId, [FromBody] ServiceStatusDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User is not authenticated.");
            var result = await _serviceManager.ServiceRequestService.UpdateServiceStatusAsync(requestId, userId, dto);
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

}
