using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Stockat.API.ActionFilters;
using Stockat.Core;
using Stockat.Core.DTOs.UserDTOs;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;

namespace Stockat.API.Controllers;

[Route("api/authentication")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IServiceManager _service;
    public AuthenticationController(IServiceManager service) => _service = service;

    [HttpPost("register")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
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
        return StatusCode(201, new { message = "User registered successfully." });
    }

    [HttpPost("login")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> Authenticate([FromBody] UserForAuthenticationDto user)
    {
        var status = await _service.AuthenticationService.ValidateUser(user);
        TokenDto tokenDto;

        switch (status)
        {
            case AuthenticationStatus.InvalidCredentials:
                return Unauthorized(new { message = "Invalid username or password." });

            case AuthenticationStatus.EmailNotConfirmed:
                return BadRequest(new { message = "Email not confirmed." });

            case AuthenticationStatus.AccountDeleted:
                tokenDto = await _service.AuthenticationService.CreateToken(true);
                return Ok(new AuthResponseDto
                {
                    Token = tokenDto,
                    IsAuthSuccessful = true,
                    IsDeleted = true
                });

            case AuthenticationStatus.Success:
                tokenDto = await _service.AuthenticationService.CreateToken(true);
                return Ok(new AuthResponseDto
                {
                    Token = tokenDto,
                    IsAuthSuccessful = true,
                    IsDeleted = false
                });

            default:
                return StatusCode(500, "Unexpected authentication error.");
        }
    }




    [HttpPost("googleLogin")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalAuthDto externalAuth)
    {
        var status = await _service.AuthenticationService.ExternalLoginAsync(externalAuth);
        TokenDto tokenDto;

        switch (status)
        {
            case AuthenticationStatus.InvalidCredentials:
                return Unauthorized(new { message = "Google login failed." });

            case AuthenticationStatus.AccountDeleted:
                tokenDto = await _service.AuthenticationService.CreateToken(true);
                return Ok(new AuthResponseDto
                {
                    Token = tokenDto,
                    IsAuthSuccessful = true,
                    IsDeleted = true
                });

            case AuthenticationStatus.Success:
                tokenDto = await _service.AuthenticationService.CreateToken(true);
                return Ok(new AuthResponseDto
                {
                    Token = tokenDto,
                    IsAuthSuccessful = true,
                    IsDeleted = false
                });

            default:
                return StatusCode(500, "Unexpected external login error.");
        }
    }


    [HttpGet("confirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        await _service.AuthenticationService.ConfirmEmail(userId, token);
        return Ok(new { message = "Email confirmed successfully." });
    }

    [HttpPost("forgotPassword")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _service.AuthenticationService.ForgotPasswordAsync(dto.Email);
        return Ok(new { message = "Password reset link sent to email." });
    }

    [HttpPost("resetPassword")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await _service.AuthenticationService.ResetPasswordAsync(dto.Email, dto.Token, dto.Password);
        return Ok(new { message = "Password has been reset." });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _service.AuthenticationService.LogoutAsync(User.Identity.Name);
        return Ok(new { message = "Logged out successfully." });
    }
}