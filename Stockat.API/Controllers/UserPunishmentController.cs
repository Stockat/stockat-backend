using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core;
using Stockat.Core.DTOs.UserPunishmentDTOs;
using Stockat.API.ActionFilters;
using System.Security.Claims;

namespace Stockat.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UserPunishmentController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public UserPunishmentController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    // POST: api/UserPunishment
    [HttpPost]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> CreatePunishment([FromBody] CreatePunishmentDto dto)
    {
        var response = await _serviceManager.UserPunishmentService.CreatePunishmentAsync(dto);
        return StatusCode(response.Status, response);
    }

    // GET: api/UserPunishment/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPunishmentById(int id)
    {
        var response = await _serviceManager.UserPunishmentService.GetPunishmentByIdAsync(id);
        return StatusCode(response.Status, response);
    }

    // GET: api/UserPunishment/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserPunishments(string userId)
    {
        var response = await _serviceManager.UserPunishmentService.GetUserPunishmentsAsync(userId);
        return StatusCode(response.Status, response);
    }

    // GET: api/UserPunishment
    [HttpGet]
    public async Task<IActionResult> GetAllPunishments([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string searchTerm = null)
    {
        var response = await _serviceManager.UserPunishmentService.GetAllPunishmentsAsync(page, size, searchTerm);
        return StatusCode(response.Status, response);
    }

    // DELETE: api/UserPunishment/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemovePunishment(int id)
    {
        var response = await _serviceManager.UserPunishmentService.RemovePunishmentAsync(id);
        return StatusCode(response.Status, response);
    }

    // GET: api/UserPunishment/check/{userId}
    [HttpGet("check/{userId}")]
    public async Task<IActionResult> CheckIfUserBlocked(string userId)
    {
        var response = await _serviceManager.UserPunishmentService.IsUserBlockedAsync(userId);
        return StatusCode(response.Status, response);
    }

    // GET: api/UserPunishment/current/{userId}
    [HttpGet("current/{userId}")]
    public async Task<IActionResult> GetCurrentPunishment(string userId)
    {
        var response = await _serviceManager.UserPunishmentService.GetCurrentPunishmentAsync(userId);
        return StatusCode(response.Status, response);
    }

    // GET: api/UserPunishment/statistics
    [HttpGet("statistics")]
    public async Task<IActionResult> GetPunishmentStatistics()
    {
        var response = await _serviceManager.UserPunishmentService.GetPunishmentStatisticsAsync();
        return StatusCode(response.Status, response);
    }

    // GET: api/UserPunishment/active
    [HttpGet("active")]
    public async Task<IActionResult> GetActivePunishments([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var response = await _serviceManager.UserPunishmentService.GetActivePunishmentsAsync(page, size);
        return StatusCode(response.Status, response);
    }

    // GET: api/UserPunishment/type/{type}
    [HttpGet("type/{type}")]
    public async Task<IActionResult> GetPunishmentsByType(string type, [FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var response = await _serviceManager.UserPunishmentService.GetPunishmentsByTypeAsync(type, page, size);
        return StatusCode(response.Status, response);
    }
} 