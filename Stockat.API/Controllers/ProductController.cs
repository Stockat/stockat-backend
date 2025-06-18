using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core;
using Stockat.Core.DTOs.ProductDTOs;
using Stockat.Core.IServices;

namespace Stockat.API.Controllers;

[Route("api/[controller]")]
[ApiController()]
public class ProductController : ControllerBase
{
    private readonly ILoggerManager _logger;
    private readonly IServiceManager _serviceManager;

    public ProductController(ILoggerManager logger, IServiceManager serviceManager)
    {
        _logger = logger;
        _serviceManager = serviceManager;
    }


    [HttpGet]
    public async Task<IActionResult> getAllProductsPaginatedAsync(int size, int page)
    {

        var res = await _serviceManager.ProductService.getAllProductsPaginated(size, page);
        return Ok(res);
    }

    [HttpGet("/id")]
    public async Task<IActionResult> getproductDetailsAsync(int id)
    {

        var res = await _serviceManager.ProductService.GetProductDetailsAsync(id);
        return Ok(res);
    }

    [HttpPost]
    public async Task<IActionResult> Addproduct(AddProductDto productDto)
    {
        var res = await _serviceManager.ProductService.AddProductAsync(productDto);
        return Ok(res);
    }



}
