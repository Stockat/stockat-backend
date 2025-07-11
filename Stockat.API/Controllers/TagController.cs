using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core.IServices;
using Stockat.Core;
using Stockat.Core.DTOs.CategoryDtos;
using Stockat.Core.DTOs.TagsDtos;

namespace Stockat.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TagController : ControllerBase
{
    private readonly ILoggerManager _logger;
    private readonly IServiceManager _serviceManager;

    public TagController(ILoggerManager logger, IServiceManager serviceManager)
    {
        _logger = logger;
        _serviceManager = serviceManager;
    }

    [HttpGet]
    public async Task<IActionResult> getAllActiveTags()
    {

        var res = await _serviceManager.TagService.getAllActiveTags();

        return Ok(res);
    }

    [HttpGet("admin")]
    public async Task<IActionResult> getAllTags()
    {

        var res = await _serviceManager.TagService.getAllTags();

        return Ok(res);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddTag([FromBody] string tagName)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _serviceManager.TagService.AddTag(tagName);
        return Ok(res);

    }
    [HttpPost("edit/{id}")]
    public async Task<IActionResult> EditTag(int id, UpdateTagDto updateTagDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _serviceManager.TagService.EditTag(id, updateTagDto);
        return Ok(res);
    }
    [HttpPost("changeStatus/{id}")]
    public async Task<IActionResult> ChangeTagStatus(int id,[FromQuery] TagStatus status)
    {

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _serviceManager.TagService.ChangeTagStatus(id, status);
        return Ok(res);
    }
}
