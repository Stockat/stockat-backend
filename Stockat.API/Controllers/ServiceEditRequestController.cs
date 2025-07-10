using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockat.API.ActionFilters;
using Stockat.Core;
using Stockat.Core.DTOs.ServiceDTOs;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;
using System.Security.Claims;

namespace Stockat.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServiceEditRequestController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public ServiceEditRequestController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    // Seller submits an edit request
    [HttpPost("{serviceId:int}")]
    [Authorize(Roles = "Seller")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> SubmitEditRequest(int serviceId, [FromBody] CreateServiceEditRequestDto dto)
    {
        try
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(sellerId))
                return Unauthorized("Seller ID is required.");
            await _serviceManager.ServiceEditRequestService.SubmitEditRequestAsync(serviceId, sellerId, dto);
            return Ok(new { message = "Edit request submitted successfully." });
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    // Admin views all pending edit requests
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingEditRequests([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        try
        {
            var requests = await _serviceManager.ServiceEditRequestService.GetPendingRequestsAsync(page, size);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    // Admin views all approved edit requests
    [HttpGet("approved")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetApprovedEditRequests([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        try
        {
            var requests = await _serviceManager.ServiceEditRequestService.GetApprovedRequestsAsync(page, size);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    // Admin views all rejected edit requests
    [HttpGet("rejected")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRejectedEditRequests([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        try
        {
            var requests = await _serviceManager.ServiceEditRequestService.GetRejectedRequestsAsync(page, size);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    // Admin gets request statistics
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRequestStatistics()
    {
        try
        {
            Console.WriteLine("Statistics endpoint called");
            var statistics = await _serviceManager.ServiceEditRequestService.GetRequestStatisticsAsync();
            Console.WriteLine($"Statistics returned: {System.Text.Json.JsonSerializer.Serialize(statistics)}");
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Statistics error: {ex.Message}");
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    // Admin approves an edit request
    [HttpPost("{requestId:int}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveEditRequest(int requestId)
    {
        try
        {
            await _serviceManager.ServiceEditRequestService.ApproveEditRequestAsync(requestId);
            return Ok(new { message = "Edit request approved and service updated." });
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    // Admin rejects an edit request
    [HttpPost("{requestId:int}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectEditRequest(int requestId, [FromBody] RejectRequestDto dto)
    {
        try
        {
            await _serviceManager.ServiceEditRequestService.RejectEditRequestAsync(requestId, dto.Note);
            return Ok(new { message = "Edit request rejected." });
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    // Admin manually triggers deferred edit application (for testing)
    [HttpPost("apply-deferred/{serviceId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApplyDeferredEdits(int serviceId)
    {
        try
        {
            await _serviceManager.ServiceEditRequestService.ApplyDeferredEditsAsync(serviceId);
            return Ok(new { message = "Deferred edits applied successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    // Admin checks deferred edit status
    [HttpGet("deferred-status/{serviceId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDeferredEditStatus(int serviceId)
    {
        try
        {
            var status = await _serviceManager.ServiceEditRequestService.GetDeferredEditStatusAsync(serviceId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    // Seller reactivates a rejected service
    [HttpPost("reactivate/{serviceId:int}")]
    [Authorize(Roles = "Seller")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> ReactivateRejectedService(int serviceId, [FromBody] CreateServiceEditRequestDto dto)
    {
        try
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(sellerId))
                return Unauthorized("Seller ID is required.");
            
            await _serviceManager.ServiceEditRequestService.ReactivateRejectedServiceAsync(serviceId, sellerId, dto);
            return Ok(new { message = "Service reactivation request submitted successfully." });
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
} 