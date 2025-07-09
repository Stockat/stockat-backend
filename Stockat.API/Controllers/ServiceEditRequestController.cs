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
    public async Task<IActionResult> SubmitEditRequest(int serviceId, [FromBody] ServiceEditRequestDto dto)
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
    public async Task<IActionResult> GetPendingEditRequests()
    {
        try
        {
            var requests = await _serviceManager.ServiceEditRequestService.GetPendingRequestsAsync();
            return Ok(requests);
        }
        catch (Exception ex)
        {
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
    public async Task<IActionResult> RejectEditRequest(int requestId, [FromBody] string note)
    {
        try
        {
            await _serviceManager.ServiceEditRequestService.RejectEditRequestAsync(requestId, note);
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
} 