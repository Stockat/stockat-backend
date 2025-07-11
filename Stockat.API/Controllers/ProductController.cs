using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
        ([FromQuery] int size, [FromQuery] int[] tags, [FromQuery] int page = 0, string location = "", int category = 0, int minQuantity = 0, int minPrice = 0)
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
    [Authorize]
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
            productDto.Images.Add(new AddProductmageDto() { ImageUrl = img.Url, FileId = img.FileId });
        }



        var res = await _serviceManager.ProductService.AddProductAsync(productDto);
        return Ok(res);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Updateproduct([FromRoute] int id, [FromForm] string productJson, [FromForm] IFormFile[] images, [FromForm] string removedimages)
    {
        var imgUrls = await _serviceManager.ProductService.UploadProductImages(images);


        var productDto = JsonSerializer.Deserialize<UpdateProductDto>(
     productJson,
     new JsonSerializerOptions
     {
         PropertyNameCaseInsensitive = true
     });
        var removedimagesdto = JsonSerializer.Deserialize<IEnumerable<UpdateProductImageDto>>(
     removedimages,
     new JsonSerializerOptions
     {
         PropertyNameCaseInsensitive = true
     });

        foreach (var img in removedimagesdto)
        {
            await _serviceManager.ImageService.DeleteImageAsync(img.FileId);
        }

        foreach (var img in imgUrls.Data)
        {
            productDto.Images.Add(new UpdateProductImageDto() { ImageUrl = img.Url, FileId = img.FileId });
        }

        var sellerId = await _serviceManager.UserService.GetCurrentUserIdAsyncService();

        var res = await _serviceManager.ProductService.UpdateProduct(id, productDto, sellerId);
        return Ok(res);
    }


    [HttpGet("seller/{id:int}")]
    [Authorize]

    public async Task<IActionResult> getproductForUpdatesAsync(int id)
    {
        var sellerId = await _serviceManager.UserService.GetCurrentUserIdAsyncService();
        var res = await _serviceManager.ProductService.GetProductForUpdateAsync(id, sellerId);
        return Ok(res);
    }

    [HttpGet("seller")]
    [Authorize]

    public async Task<IActionResult> getAllproductForSellerAsync
    ([FromQuery] int[] tags, string location = "", int category = 0, int minQuantity = 0, int minPrice = 0, int size = 8, int page = 0)
    {
        var sellerId = await _serviceManager.UserService.GetCurrentUserIdAsyncService();
        var res = await _serviceManager.ProductService.GetAllProductForSellerAsync(size, page, location, category, minQuantity, minPrice, tags, sellerId);
        return Ok(res);
    }

    [HttpPost("{id:int}")]
    [Authorize]
    public async Task<IActionResult> ChangeProductStatus(int id, [FromBody] ChangeProductStatusDto dto)

    {
        var res = await _serviceManager.ProductService.ChangeProductStatus(id, dto.ChosenStatus);

        return Ok(res);
    }

    // Admin
    [HttpPost("reason/{id:int}")]
    [Authorize]
    public async Task<IActionResult> ChangeProductStatusWithReason(int id, [FromBody] ChangeProductStatusWithReasonDto dto)

    {
        var res = await _serviceManager.ProductService.ChangeProductStatusWithReason(id, dto.ChosenStatus, dto.Reason);

        return Ok();
    }




    [HttpPost("seller/delete")]
    [Authorize]
    public async Task<IActionResult> removeSellerProduct([FromBody] int id)

    {
        var res = await _serviceManager.ProductService.RemoveProduct(id);
        return Ok(res);
    }
    [HttpPost("seller/edit-canBeRequested")]
    [Authorize]
    public async Task<IActionResult> changeCanBeRequsted([FromBody] int id)

    {
        var sellerId = await _serviceManager.UserService.GetCurrentUserIdAsyncService();
        var res = await _serviceManager.ProductService.ChangeCanBeRequested(id, sellerId);
        return Ok(res);
    }

    // Get product with features by ID
    [HttpGet("with-features/{id:int}")]
    public async Task<IActionResult> GetProductWithFeatures(int id)
    {
        var res = await _serviceManager.ProductService.GetProductWithFeaturesAsync(id);
        return Ok(res);
    }

    // Admin Region [Authorized] ----------------------------------WatchOut--------------------------------------------------------
    [HttpGet("admin")]
    [Authorize]
    public async Task<IActionResult> getAllProductsPaginatedForAdminAsync
     ([FromQuery] int size, [FromQuery] int[] tags, [FromQuery] int page = 0, string location = "", int category = 0, int minQuantity = 0, int minPrice = 0)
    {

        var res = await _serviceManager.ProductService.getAllProductsPaginatedForAdmin(size, page, location, category, minQuantity, minPrice, tags);
        return Ok(res);
    }

    [HttpGet("admin/product-with-stocks/{id:int}")]
    public async Task<IActionResult> GetProductWithStocksForAdmin(int id)
    {
        var res = await _serviceManager.ProductService.GetProductWithStocksForAdminAsync(id);
        return Ok(res);
    }

    [HttpGet("{sellerId}")]
    public async Task<IActionResult> GetSpecificSellerProducts
    (string sellerId, [FromQuery] int[] tags, string location = "", int category = 0, int minQuantity = 0, int minPrice = 0, int size = 9, int page = 0)
    {
        var res = await _serviceManager.ProductService.GetAllProductForSellerAsync(size, page, location, category, minQuantity, minPrice, tags, sellerId);
        return Ok(res);
    }

}
