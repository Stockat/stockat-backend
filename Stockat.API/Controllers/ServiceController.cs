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
    [Authorize]
    public async Task<IActionResult> GetServiceById(int serviceId)
    {
        try
        {
            var service = await _service.ServiceService.GetServiceByIdAsync(serviceId);
            return Ok(service);
        } 
        catch (NotFoundException ex)
        {
            return NotFound($"Service with ID {serviceId} not found: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving the service: {ex.Message}");
        }
    }

    [HttpDelete("{serviceId:int}")]
    [Authorize(Roles = "Seller, Admin")]
    public async Task<IActionResult> DeleteService(int serviceId)
    {
        try
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(sellerId))
                return Unauthorized("Seller ID is required.");


            await _service.ServiceService.DeleteAsync(serviceId, sellerId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid($"You do not have permission to delete this service: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting the service: {ex.Message}");
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllAvailableServices([FromQuery] int page = 0, [FromQuery] int size = 10)
    {
        var services = await _service.ServiceService.GetAllAvailableServicesAsync(page,size);
        return Ok(services);
    }

    [HttpGet("seller/{sellerId}")]
    [Authorize]
    public async Task<IActionResult> GetSellerServices(string sellerId, [FromQuery] int page = 0, [FromQuery] int size = 10)
    {
        if (string.IsNullOrEmpty(sellerId))
            return BadRequest("Seller ID is required.");


        var services = await _service.ServiceService.GetSellerServicesAsync(sellerId, page, size);
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
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sellerId))
            return Unauthorized("Seller ID is required.");

        var updatedService = await _service.ServiceService.UpdateAsync(serviceId, dto, sellerId);
        return Ok(updatedService);
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
}