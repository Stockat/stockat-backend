using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockat.API.ActionFilters;
using Stockat.Core;
using Stockat.Core.DTOs.UserDTOs;
using Stockat.Core.Exceptions;

namespace Stockat.API.Controllers;

[Route("api/authentication")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IServiceManager _service;
    public AuthenticationController(IServiceManager service) => _service = service;

    [HttpPost("register")]
    [ServiceFilter(typeof(ValidationFilterAttribute))] // custom filter read the below to understand
    /*
        Automatically returns 400 if the DTO is null
        Returns 422 if model validation fails
        Keeps controllers clean by centralizing validation logic
     */
    public async Task<IActionResult> RegisterUser([FromBody] UserForRegistrationDto userForRegistration)
    {
        var result = await _service.AuthenticationService.RegisterUser(userForRegistration);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.TryAddModelError(error.Code, error.Description);
            }
            return BadRequest(ModelState);
        }
        return StatusCode(201); // created
    }

    [HttpPost("login")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> Authenticate([FromBody] UserForAuthenticationDto user)
    {
        if (!await _service.AuthenticationService.ValidateUser(user))
            return Unauthorized();
        var tokenDto = await _service.AuthenticationService.CreateToken( true);
        return Ok(new AuthResponseDto
        {
            Token = tokenDto,
            IsAuthSuccessful = true
        });
    }

    [HttpPost("googleLogin")] // google external login
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalAuthDto externalAuth)
    {
        var tokenDto = await _service.AuthenticationService.ExternalLoginAsync(externalAuth);
        return Ok(new AuthResponseDto
        {
            Token = tokenDto,
            IsAuthSuccessful = true
        });
    }

    [HttpGet("confirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        await _service.AuthenticationService.ConfirmEmail(userId, token);
        return Ok("Email confirmed successfully.");
    }

    [HttpPost("forgotPassword")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _service.AuthenticationService.ForgotPasswordAsync(dto.Email);
        return Ok("Password reset link sent to email.");
    }

    [HttpPost("resetPassword")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await _service.AuthenticationService.ResetPasswordAsync(dto.Email, dto.Token, dto.Password);
        return Ok("Password has been reset.");
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _service.AuthenticationService.LogoutAsync(User.Identity.Name);
        return Ok("Logged out successfully.");
    }


}