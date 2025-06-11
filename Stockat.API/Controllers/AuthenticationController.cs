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


    [HttpPost]
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

    [HttpPost("ExternalLogin")] // google external login
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalAuthDto externalAuth)
    {
        var tokenDto = await _service.AuthenticationService.ExternalLoginAsync(externalAuth);
        return Ok(new AuthResponseDto
        {
            Token = tokenDto,
            IsAuthSuccessful = true
        });
    }


}