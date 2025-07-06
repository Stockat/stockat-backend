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

    //  api/UserVerification
    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        var response = await _serviceManager.UserVerificationService.DeleteAsync();
        return StatusCode(response.Status, response);
    }

    // api/UserVerification/status
    [HttpPut("status")]
    [Authorize(Roles = "Admin")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> UpdateStatus([FromBody] UserVerificationStatusUpdateDto dto)
    {
        var response = await _serviceManager.UserVerificationService.UpdateStatusByAdminAsync(dto);
        return StatusCode(response.Status, response);
    }

    // Admin-specific endpoints
    // GET: api/UserVerification/admin/pending
    [HttpGet("admin/pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingVerifications([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var response = await _serviceManager.UserVerificationService.GetPendingVerificationsAsync(page, size);
        return StatusCode(response.Status, response);
    }

    // GET: api/UserVerification/admin/statistics
    [HttpGet("admin/statistics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetVerificationStatistics()
    {
        var response = await _serviceManager.UserVerificationService.GetVerificationStatisticsAsync();
        return StatusCode(response.Status, response);
    }
}
