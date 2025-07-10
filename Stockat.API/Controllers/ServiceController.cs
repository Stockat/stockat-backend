using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockat.API.ActionFilters;
using Stockat.Core;
using Stockat.Core.DTOs.ServiceDTOs;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;
using Stockat.Service;

namespace Stockat.API.Controllers;


[Route("api/[controller]")]
[ApiController]
public class ServiceController : ControllerBase
{
    private readonly IServiceManager _service;

    public ServiceController(IServiceManager service)
    {
        _service = service;

    }

    [HttpPost]
    [Authorize(Roles = "Seller, Admin")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceDto dto)
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("Seller ID is required.");
        
        var result = await _service.ServiceService.CreateAsync(dto, sellerId);
        return CreatedAtAction(nameof(GetServiceById), new { serviceId = result.Id }, result);
    }

    [HttpGet("{serviceId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetServiceById(int serviceId)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;

            var service = await _service.ServiceService.GetServiceByIdAsync(serviceId, userId);
            return Ok(service);
        } 
        catch (NotFoundException ex)
        {
            return NotFound($"{ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving the service: {ex.Message}");
        }
    }

    // soft delete
    [HttpPatch("{serviceId:int}/delete")]
    [Authorize(Roles = "Seller, Admin")]
    public async Task<IActionResult> DeleteService(int serviceId)
    {
        try
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userID))
                return Unauthorized("Seller ID is required.");

            // Check if user is admin
            bool isAdmin = User.IsInRole("Admin");

            await _service.ServiceService.DeleteAsync(serviceId, userID, isAdmin);
            return NoContent();
        }
        catch (BadRequestException ex)
        {
            return BadRequest($"Cannot delete service: {ex.Message}");
        }
        catch (NotFoundException ex)
        {
            return NotFound($"Service with ID {serviceId} not found: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting the service: {ex.Message}");
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllAvailableServices([FromQuery] int page = 0, [FromQuery] int size = 10)
    {
        var services = await _service.ServiceService.GetAllAvailableServicesAsync(page,size);
        return Ok(services);
    }

    [HttpGet("seller/{sellerId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSellerServices(string sellerId, [FromQuery] int page = 0, [FromQuery] int size = 10)
    {
        if (string.IsNullOrEmpty(sellerId))
            return BadRequest("Seller ID is required.");


        var services = await _service.ServiceService.GetSellerServicesAsync(sellerId, page, size, isPublicView: true);
        if (services == null)
            return NotFound($"No services found for seller with ID {sellerId}.");

        return Ok(services);
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetMyServices([FromQuery] int page = 0, [FromQuery] int size = 10)
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("Seller ID is required.");

        var services = await _service.ServiceService.GetSellerServicesAsync(sellerId, page, size);
        if (services == null)
            return NotFound("No services found for the current seller.");

        return Ok(services);
    }


    [HttpPatch("{serviceId:int}")]
    [Authorize(Roles = "Seller, Admin")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> UpdateService(int serviceId, [FromBody] UpdateServiceDto dto)
    {
        // Sellers must use the edit request flow. Only Admin can update directly (if needed).
        return Forbid("Sellers must submit an edit request instead of direct update.");
    }


    [HttpPost("{serviceId:int}/upload-image")]
    [Authorize(Roles = "Seller, Admin")]
    public async Task<IActionResult> UploadServiceImage(int serviceId, IFormFile file)
    {
        if (file == null)
            return BadRequest("No file was provided.");

        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("You must be logged in to upload an image.");

        try
        {
            var result = await _service.ServiceService.UploadServiceImageAsync(serviceId, sellerId, file);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Image upload failed: {ex.Message}");
        }
    }

    [HttpPost("upload-image")]
    [Authorize]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null)
            return BadRequest("No file was provided.");
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("You must be logged in to upload an image.");

        try
        {
            var result = await _service.ImageService.UploadImageAsync(file, "Services");
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Image upload failed: {ex.Message}");
        }
    }

   [HttpPatch("approve/{serviceId:int}")]
   [Authorize(Roles = "Admin")]
   public async Task<IActionResult> ApproveService(int serviceId, [FromBody] bool isApproved)
   {
       try
       {
           var updatedService = await _service.ServiceService.UpdateApprovalStatusAsync(serviceId, isApproved);
           return Ok(new { 
               message = $"Service {serviceId} approval status updated to {(isApproved ? "approved" : "rejected")}",
               service = updatedService
           });
       }
       catch (NotFoundException ex)
       {
           return NotFound(ex.Message);
       }
       catch (Exception ex)
       {
           return StatusCode(500, $"An error occurred while updating service approval status: {ex.Message}");
       }
   }

    [HttpGet("admin/pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingServicesForApproval([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var result = await _service.ServiceService.GetAllAvailableServicesAsync(page, size, pendingOnly: true);
        return Ok(result);
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllServicesForAdmin(
        [FromQuery] int page = 1, 
        [FromQuery] int size = 10,
        [FromQuery] bool? includeBlockedSellers = null,
        [FromQuery] bool? includeDeletedSellers = null,
        [FromQuery] bool? includeDeletedServices = null)
    {
        var result = await _service.ServiceService.GetAllServicesForAdminAsync(page, size, includeBlockedSellers, includeDeletedSellers, includeDeletedServices);
        return Ok(result);
    }
}