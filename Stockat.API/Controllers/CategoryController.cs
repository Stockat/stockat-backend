using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core.IServices;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.CategoryDtos;

namespace Stockat.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ILoggerManager _logger;
    private readonly IServiceManager _serviceManager;

    public CategoryController(ILoggerManager logger, IServiceManager serviceManager)
    {
        _logger = logger;
        _serviceManager = serviceManager;
    }

    [HttpGet]
    public async Task<IActionResult> getAllCategories()
    {

        var res = await _serviceManager.CategoryService.getAllCategory();

        return Ok(res);
    }

}
