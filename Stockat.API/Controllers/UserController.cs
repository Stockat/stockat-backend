﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core;
using Stockat.Core.DTOs.UserDTOs;
using Stockat.API.ActionFilters;

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
}
