using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core.DTOs.UserVerificationDTOs;
using Stockat.Core;
using System.Threading.Tasks;
using Stockat.API.ActionFilters;

namespace Stockat.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserVerificationController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public UserVerificationController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    // api/UserVerification
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { Message = "User ID not found in token." });

        var response = await _serviceManager.UserVerificationService.GetByUserIdAsync(userId);
        if (response.Status == 404)
            return NotFound(response);

        return Ok(response);
    }

    // api/UserVerification
    [HttpPost]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> Create([FromForm] UserVerificationCreateDto dto)
    {
        var response = await _serviceManager.UserVerificationService.CreateAsync(dto);
        return StatusCode(response.Status, response);
    }

    // api/UserVerification
    [HttpPut]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> Update([FromForm] UserVerificationUpdateDto dto)
    {
        var response = await _serviceManager.UserVerificationService.UpdateAsync(dto);
        return StatusCode(response.Status, response);
    }

    // DELETE: api/UserVerification
    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        var response = await _serviceManager.UserVerificationService.DeleteAsync();
        return StatusCode(response.Status, response);
    }
}
