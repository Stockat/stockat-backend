using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.MediaDTOs;
using Stockat.Core.DTOs.ProductDTOs;
using Stockat.Core.DTOs.ProductImageDto;
using Stockat.Core.Enums;
using Stockat.Core.IServices;
using System.Text.Json;

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

    // Anonymous  Users ---------------------------------------------------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> getAllProductsPaginatedAsync
        ([FromQuery] int[] tags, string location = "", int category = 0, int minQuantity = 0, int minPrice = 0, int size = 9, int page = 1)
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




    // Seller Region [Authorized] ----------------------------------WatchOut--------------------------------------------------------

    [HttpPost]
    public async Task<IActionResult> Addproduct([FromForm] string productJson, [FromForm] IFormFile[] images)
    {

        var imgUrls = await _serviceManager.ProductService.UploadProductImages(images);


        var productDto = JsonSerializer.Deserialize<AddProductDto>(
     productJson,
     new JsonSerializerOptions
     {
         PropertyNameCaseInsensitive = true
     });

        foreach (var img in imgUrls.Data)
        {
            productDto.Images.Add(new AddProductmageDto() { ImageUrl = img });
        }



        var res = await _serviceManager.ProductService.AddProductAsync(productDto);
        return Ok(res);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Updateproduct([FromRoute] int id, [FromForm] string productJson, [FromForm] IFormFile[] images)
    {
        var imgUrls = await _serviceManager.ProductService.UploadProductImages(images);


        var productDto = JsonSerializer.Deserialize<UpdateProductDto>(
     productJson,
     new JsonSerializerOptions
     {
         PropertyNameCaseInsensitive = true
     });

        foreach (var img in imgUrls.Data)
        {
            productDto.Images.Add(new UpdateProductImageDto() { ImageUrl = img });
        }

        var res = await _serviceManager.ProductService.UpdateProduct(id, productDto);
        return Ok(res);
    }


    [HttpGet("seller/{id:int}")]
    public async Task<IActionResult> getproductForUpdatesAsync(int id)
    {

        var res = await _serviceManager.ProductService.GetProductForUpdateAsync(id);
        return Ok(res);
    }

    [HttpGet("seller")]
    public async Task<IActionResult> getAllproductForSellerAsync
    ([FromQuery] int[] tags, string location = "", int category = 0, int minQuantity = 0, int minPrice = 0, int size = 9, int page = 1)
    {

        var res = await _serviceManager.ProductService.GetAllProductForSellerAsync(size, page, location, category, minQuantity, minPrice, tags);
        return Ok(res);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> ChangeProductStatus(int id, [FromBody] ChangeProductStatusDto dto)

    {
        var res = await _serviceManager.ProductService.ChangeProductStatus(id, dto.ChosenStatus);

        return Ok(res);
    }
    [HttpPost("seller/delete")]
    public async Task<IActionResult> removeSellerProduct([FromBody] int id)

    {
        var res = await _serviceManager.ProductService.RemoveProduct(id);
        return Ok(res);
    }
    [HttpPost("seller/edit-canBeRequested")]
    public async Task<IActionResult> changeCanBeRequsted([FromBody] int id)

    {
        var res = await _serviceManager.ProductService.ChangeCanBeRequested(id);
        return Ok(res);
    }



}
