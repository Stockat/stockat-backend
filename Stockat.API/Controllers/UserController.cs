using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockat.API.ActionFilters;
using Stockat.Core;
using Stockat.Core.DTOs.UserDTOs;
using System.Security.Claims;

namespace Stockat.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public UserController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }
    // GET: api/user/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var response = await _serviceManager.UserService.GetUserAsync(id);
        return StatusCode(response.Status, response);
    }
    // GET: api/User
    [HttpGet]
    public async Task<IActionResult> GetCurrentUser()
    
    {
        var response = await _serviceManager.UserService.GetUserAsync(); // Get current user by token
        return StatusCode(response.Status, response);
    }

    // PUT: api/User
    [HttpPut]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto dto)
    {
        var response = await _serviceManager.UserService.UpdateAsync(dto);
        return StatusCode(response.Status, response);
    }

    // PUT: api/User/profile-image
    [HttpPut("profile-image")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> UpdateProfileImage([FromForm] UserImageUpdateDto dto)
    {
        var response = await _serviceManager.UserService.UpdateProfileImageAsync(dto);
        return StatusCode(response.Status, response);
    }

    // PUT: api/User/change-password
    [HttpPut("change-password")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var response = await _serviceManager.UserService.ChangePasswordAsync(dto);
        return StatusCode(response.Status, response);
    }

    // PUT: api/User/toggle-activation
    [HttpPut("toggle-activation")]
    public async Task<IActionResult> ToggleActivation()
    {
        var response = await _serviceManager.UserService.ToggleActivationAsync();
        return StatusCode(response.Status, response);
    }

    // Admin-specific endpoints
    // GET: api/User/admin/all
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int page = 1, 
        [FromQuery] int size = 10, 
        [FromQuery] string? searchTerm = null, 
        [FromQuery] bool? isActive = null, 
        [FromQuery] bool? isVerified = null,
        [FromQuery] bool? isBlocked = null)
    {
        var response = await _serviceManager.UserService.GetAllUsersAsync(page, size, searchTerm, isActive, isVerified, isBlocked);
        return StatusCode(response.Status, response);
    }

    // GET: api/User/admin/{userId}/details
    [HttpGet("admin/{userId}/details")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserWithDetails(string userId)
    {
        var response = await _serviceManager.UserService.GetUserWithDetailsAsync(userId);
        return StatusCode(response.Status, response);
    }

    // PUT: api/User/admin/{userId}/deactivate
    [HttpPut("admin/{userId}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateUser(string userId)
    {
        var response = await _serviceManager.UserService.DeactivateUserAsync(userId);
        return StatusCode(response.Status, response);
    }

    // PUT: api/User/admin/{userId}/activate
    [HttpPut("admin/{userId}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActivateUser(string userId)
    {
        var response = await _serviceManager.UserService.ActivateUserAsync(userId);
        return StatusCode(response.Status, response);
    }

    // GET: api/User/admin/statistics
    [HttpGet("admin/statistics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserStatistics()
    {
        var response = await _serviceManager.UserService.GetUserStatisticsAsync();
        return StatusCode(response.Status, response);
    }

    [HttpPost("upgrade-to-seller")]
    public async Task<IActionResult> UpgradeToSeller()
    {
        var result = await _serviceManager.UserService.UpgradeToSellerAsync();
        if (result.Status != 200)
            return StatusCode(result.Status, result);

        // Generate a new token with the new role
        var userId = GetCurrentUserId();
        var tokenDto = await _serviceManager.AuthenticationService.CreateToken(true, userId);
        return Ok(new { message = result.Message, token = tokenDto });
    }

    private string GetCurrentUserId()
    {
        var userId = HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User ID not found in token.");
        return userId;
    }
}
