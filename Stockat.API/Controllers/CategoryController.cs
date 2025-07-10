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

        var res = await _serviceManager.CategoryService.getAllActiveCategory();

        return Ok(res);
    }

    [HttpGet("admin")]
    public async Task<IActionResult> getAllCategoriesForAdmin()
    {

        var res = await _serviceManager.CategoryService.getAllCategory();

        return Ok(res);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddCategory([FromBody] string categoryName)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _serviceManager.CategoryService.AddCategory(categoryName);
        return Ok(res);

    }
    [HttpPost("edit/{id}")]
    public async Task<IActionResult> EditCategory(int id, CategoryDto categoryDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _serviceManager.CategoryService.EditCategory(id, categoryDto);
        return Ok(res);
    }
    [HttpPost("delete/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _serviceManager.CategoryService.DeleteCategory(id);
        return Ok(res);
    }

}
