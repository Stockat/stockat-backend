using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.ProductDTOs;
using Stockat.Core.Enums;
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
    public async Task<IActionResult> getAllProductsPaginatedAsync
        ([FromQuery] string[] tags, string location = "", string category = "", int minQuantity = 0, int minPrice = 0, int size = 9, int page = 1)
    {

        var res = await _serviceManager.ProductService.getAllProductsPaginated(size, page, location, category, minQuantity, minPrice, tags);
        return Ok(res);
    }

    [HttpGet("{id:int}")]
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Updateproduct(int id, UpdateProductDto productDto)
    {
        var res = await _serviceManager.ProductService.UpdateProduct(id, productDto);
        return Ok(res);
    }


    [HttpPost("{id:int}")]
    public async Task<IActionResult> ChaneProductStatus(int id, ProductStatus chosenStatus)
    {
        var res = await _serviceManager.ProductService.ChangeProductStatus(id, chosenStatus);

        return Ok(res);
    }

   

}
