using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockat.API.ActionFilters;
using Stockat.Core;
using Stockat.Core.DTOs.ServiceRequestUpdateDTOs;
using Stockat.Core.Exceptions;

namespace Stockat.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServiceRequestUpdateController : ControllerBase
{
    IServiceManager _serviceManager;
    public ServiceRequestUpdateController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    [HttpPost("{requestId:int}")]
    [Authorize(Roles = "Buyer")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> CreateUpdateAsync(int requestId, [FromBody] CreateServiceRequestUpdateDto dto)
    {
       
        var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(buyerId))
            return Unauthorized("You must be logged in to create an update.");

        try
        {
            var update = await _serviceManager.ServiceRequestUpdateService.CreateUpdateAsync(requestId, buyerId, dto);
            return Created($"/api/servicerequestupdate/{update.Id}", update);
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
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPatch("seller-approval/{updateId:int}")]
    [Authorize(Roles = "Seller")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> HandleSellerApprovalAsync(int updateId, [FromBody] ServiceRequestUpdateApprovalDto dto)
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("You must be logged in to approve or reject updates.");

        try
        {
            var result = await _serviceManager.ServiceRequestUpdateService.HandleSellerApprovalAsync(updateId, sellerId, dto.Approved);
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
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("{updateId:int}")]
    [Authorize]
    public async Task<IActionResult> GetUpdateByIdAsync(int updateId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("You must be logged in to view updates.");

        try
        {
            var update = await _serviceManager.ServiceRequestUpdateService.GetUpdateByIdAsync(updateId, userId);
            return Ok(update);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("request/{requestId:int}")]
    [Authorize]
    public async Task<IActionResult> GetUpdatesByRequestIdAsync(int requestId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("You must be logged in to view updates.");
        try
        {
            var updates = await _serviceManager.ServiceRequestUpdateService.GetUpdatesByRequestIdAsync(requestId, userId);
            return Ok(updates);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPatch("{updateId:int}/cancel")]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> CancelUpdateAsync(int updateId)
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("You must be logged in to cancel updates.");

        try
        {
            var result = await _serviceManager.ServiceRequestUpdateService.CancelUpdateAsync(updateId, sellerId);
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
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

}
