using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Stockat.Core;
using Stockat.Core.Consts;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.DTOs.CategoryDtos;
using Stockat.Core.DTOs.MediaDTOs;
using Stockat.Core.DTOs.OrderDTOs;
using Stockat.Core.DTOs.ProductDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Stockat.Service.Services;

public class ProductService : IProductService
{

    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private IImageService _imageService;
    private readonly IRepositoryManager _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductService(ILoggerManager logger, IMapper mapper, IRepositoryManager repo, IImageService imageService, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
        _imageService = imageService;
        _httpContextAccessor = httpContextAccessor;
    }



    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<ProductHomeDto>>>> getAllProductsPaginated
        (int _size, int _page, string location, int category, int minQuantity, int minPrice, int[] tags)

    {
        int skip = (_page) * _size;
        int take = _size;

        var counting = await _repo.ProductRepository.CountAsync(p => p.isDeleted == false &&
            p.MinQuantity >= minQuantity &&
            p.Price >= minPrice &&
              (
            tags.Length == 0 ||
             p.ProductTags.Any(pt => tags.Contains(pt.TagId))
              ) &&
             (
             string.IsNullOrEmpty(location) ||
             p.Location.ToString().ToUpper() == location.ToUpper()
             ) &&
            (
                category == 0 ||
                p.CategoryId == category
             ) &&
             (p.ProductStatus == ProductStatus.Approved || p.ProductStatus == ProductStatus.Activated));

        var res = await _repo.ProductRepository.FindAllAsync
            (
            p => p.isDeleted == false &&
            p.MinQuantity >= minQuantity &&
            p.Price >= minPrice &&
              (
            tags.Length == 0 ||
             p.ProductTags.Any(pt => tags.Contains(pt.TagId))
              ) &&
             (
             string.IsNullOrEmpty(location) ||
             p.Location.ToString().ToUpper() == location.ToUpper()
             ) &&
            (
                category == 0 ||
                p.CategoryId == category
             ) &&
             (p.ProductStatus == ProductStatus.Approved || p.ProductStatus == ProductStatus.Activated)

            , skip: skip, take: take, includes: ["Images", "ProductTags.Tag", "Category"], o => o.Id, OrderBy.Descending
            );

        //res.TryGetNonEnumeratedCount(out var count);

        var productDtos = _mapper.Map<IEnumerable<ProductHomeDto>>(res);

        var paginatedres = new PaginatedDto<IEnumerable<ProductHomeDto>>()
        {

            PaginatedData = productDtos,
            Size = _size,
            Count = counting,
            Page = _page
        };

        var resDto = new GenericResponseDto<PaginatedDto<IEnumerable<ProductHomeDto>>>()
        {
            Data = paginatedres,
            Message = "Success",
            Status = 200,
            RedirectUrl = null,
        };

        return resDto;
    }

    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<ProductHomeDto>>>> getAllProductsPaginatedForAdmin
     (int _size, int _page, string location, int category, int minQuantity, int minPrice, int[] tags)

    {
        int skip = (_page) * _size;
        int take = _size;

        var counting = await _repo.ProductRepository.CountAsync(p =>
            p.MinQuantity >= minQuantity &&
            p.Price >= minPrice &&
              (
            tags.Length == 0 ||
             p.ProductTags.Any(pt => tags.Contains(pt.TagId))
              ) &&
             (
             string.IsNullOrEmpty(location) ||
             p.Location.ToString().ToUpper() == location.ToUpper()
             ) &&
            (
                category == 0 ||
                p.CategoryId == category
             ));

        var res = await _repo.ProductRepository.FindAllAsync
            (
            p =>
            p.MinQuantity >= minQuantity &&
            p.Price >= minPrice &&
              (
            tags.Length == 0 ||
             p.ProductTags.Any(pt => tags.Contains(pt.TagId))
              ) &&
             (
             string.IsNullOrEmpty(location) ||
             p.Location.ToString().ToUpper() == location.ToUpper()
             ) &&
            (
                category == 0 ||
                p.CategoryId == category
             )


            , skip: skip, take: take, includes: ["Images", "ProductTags.Tag"], o => o.Id, OrderBy.Descending
            );

        //res.TryGetNonEnumeratedCount(out var count);

        var productDtos = _mapper.Map<IEnumerable<ProductHomeDto>>(res);

        var paginatedres = new PaginatedDto<IEnumerable<ProductHomeDto>>()
        {

            PaginatedData = productDtos,
            Size = _size,
            Count = counting,
            Page = _page
        };

        var resDto = new GenericResponseDto<PaginatedDto<IEnumerable<ProductHomeDto>>>()
        {
            Data = paginatedres,
            Message = "Success",
            Status = 200,
            RedirectUrl = null,
        };

        return resDto;
    }






    public async Task<GenericResponseDto<ProductDetailsDto>> GetProductDetailsAsync(int id)
    {
        var res = await _repo.ProductRepository.FindProductDetailsAsync
            (
            p => p.Id == id && p.isDeleted == false, ["Images", "Stocks", "Category"]

            );

        return new GenericResponseDto<ProductDetailsDto>()
        {
            Data = res,
            Message = "Success",
            Status = 200,
            RedirectUrl = null,
        };

    }

    public async Task<GenericResponseDto<UpdateProductDto>> GetProductForUpdateAsync(int id)
    {

        //Get the user ID from the HTTP context
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError("Seller ID not found in the HTTP context.");
            return new GenericResponseDto<UpdateProductDto>
            {
                Status = 400,
                Message = "User ID is required."
            };
        }



        var res = await _repo.ProductRepository.GetProductForUpdateAsync
            (
            p => p.Id == id && p.isDeleted == false, ["Images", "Stocks", "Category"]

            );
        if (res.SellerId != userId)
        {
            return new GenericResponseDto<UpdateProductDto>()
            {
                Message = "Un Authorized Access ",
                Status = 401,
                RedirectUrl = null,
            };
        }

        return new GenericResponseDto<UpdateProductDto>()
        {
            Data = res,
            Message = "Success",
            Status = 200,
            RedirectUrl = null,
        };

    }
    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<GetSellerProductDto>>>> GetAllProductForSellerAsync
        (int _size, int _page, string location, int category, int minQuantity, int minPrice, int[] tags)
    {
        //Get the user ID from the HTTP context
        var sellerId = _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(sellerId))
        {
            _logger.LogError("User ID not found in the HTTP context.");
            return new GenericResponseDto<PaginatedDto<IEnumerable<GetSellerProductDto>>>
            {
                Status = 400,
                Message = "User ID is required."
            };
        }

        int skip = (_page) * _size;
        int take = _size;

        var counting = await _repo.ProductRepository.CountAsync(
            p => p.isDeleted == false &&
            p.SellerId == sellerId &&
            p.MinQuantity >= minQuantity &&
            p.Price >= minPrice &&
              (
            tags.Length == 0 ||
             p.ProductTags.Any(pt => tags.Contains(pt.TagId))
              ) &&
             (
             string.IsNullOrEmpty(location) ||
             p.Location.ToString().ToUpper() == location.ToUpper()
             ) &&
            (
                category == 0 ||
                p.CategoryId == category
             )
     );



        var res = await _repo.ProductRepository.FindAllAsync
            (
            p => p.isDeleted == false &&
            p.SellerId == sellerId &&
            p.MinQuantity >= minQuantity &&
            p.Price >= minPrice &&
              (
            tags.Length == 0 ||
             p.ProductTags.Any(pt => tags.Contains(pt.TagId))
              ) &&
             (
             string.IsNullOrEmpty(location) ||
             p.Location.ToString().ToUpper() == location.ToUpper()
             ) &&
            (
                category == 0 ||
                p.CategoryId == category
             )

            , skip: skip, take: take, includes: ["Images"], o => o.Id, OrderBy.Descending
            );

        var productDtos = _mapper.Map<IEnumerable<GetSellerProductDto>>(res);

        var paginatedres = new PaginatedDto<IEnumerable<GetSellerProductDto>>()
        {

            PaginatedData = productDtos,
            Size = _size,
            Count = counting,
            Page = _page
        };

        var resDto = new GenericResponseDto<PaginatedDto<IEnumerable<GetSellerProductDto>>>()
        {
            Data = paginatedres,
            Message = "Success",
            Status = 200,
            RedirectUrl = null,
        };

        return resDto;
    }

    public async Task<int> AddProductAsync(AddProductDto productDto)
    {
        var product = _mapper.Map<Product>(productDto);

        await _repo.ProductRepository.AddAsync(product);
        return await _repo.CompleteAsync();

    }
    public async Task<int> UpdateProduct(int id, UpdateProductDto productDto)
    {
        //var isProductFound = await _repo.ProductRepository.IsProductFoundAsync(p => p.Id == id);

        var oldProduct = await _repo.ProductRepository.FindAsync(p => p.Id == id && p.isDeleted == false, ["Images", "Stocks", "Category", "Features", "ProductTags"]);

        if (oldProduct == null)
            throw new NotFoundException($"Product With Id:{id} Not Found, please Contact with Admin for further information");

        _mapper.Map(productDto, oldProduct);

        _repo.ProductRepository.Update(oldProduct);

        return await _repo.CompleteAsync();

    }

    public async Task<GenericResponseDto<string>> ChangeProductStatus(int id, ProductStatus chosenStatus)
    {
        var product = await _repo.ProductRepository.FindAsync(p => p.Id == id && p.isDeleted == false);

        if (product == null)
            throw new NotFoundException($"Product With Id:{id} Not Found, please Contact with Admin for further information");

        product.ProductStatus = chosenStatus;

        await _repo.CompleteAsync();

        return new GenericResponseDto<string>()
        {
            Data = "",
            Message = "Product Status Updated",
            Status = 200
        };
    }
    public async Task<GenericResponseDto<string>> ChangeProductStatusWithReason(int id, ProductStatus chosenStatus, string reason)
    {
        var product = await _repo.ProductRepository.FindAsync(p => p.Id == id && p.isDeleted == false);

        if (product == null)
            throw new NotFoundException($"Product With Id:{id} Not Found, please Contact with Admin for further information");

        product.ProductStatus = chosenStatus;
        product.RejectionReason = reason;

        _repo.ProductRepository.Update(product);
        await _repo.CompleteAsync();

        return new GenericResponseDto<string>()
        {
            Data = "",
            Message = "Product Status Updated",
            Status = 200
        };
    }
    public async Task<GenericResponseDto<string>> RemoveProduct(int id)
    {
        var product = await _repo.ProductRepository.FindAsync(p => p.Id == id && p.isDeleted == false);

        if (product == null)
            throw new NotFoundException($"Product With Id:{id} Not Found, please Contact with Admin for further information");

        product.isDeleted = true;

        await _repo.CompleteAsync();

        return new GenericResponseDto<string>()
        {
            Data = "",
            Message = "Product Deleted Successfully",
            Status = 200
        };
    }
    public async Task<GenericResponseDto<string>> ChangeCanBeRequested(int id)
    {
        var product = await _repo.ProductRepository.FindAsync(p => p.Id == id && p.isDeleted == false);

        if (product == null)
            throw new NotFoundException($"Product With Id:{id} Not Found, please Contact with Admin for further information");

        product.canBeRequested = !product.canBeRequested;

        await _repo.CompleteAsync();

        return new GenericResponseDto<string>()
        {
            Data = "",
            Message = "Product Updated Successfully",
            Status = 200
        };
    }

    public async Task<GenericResponseDto<IEnumerable<ImageUploadResultDto>>> UploadProductImages(IFormFile[] imgs)
    {

        var uploadResult = await _imageService.UploadImagesAsync(imgs, "/ProductImages");


        return new GenericResponseDto<IEnumerable<ImageUploadResultDto>>
        {
            Message = "Profile image updated successfully.",
            Status = StatusCodes.Status200OK,
            Data = uploadResult
        };

    }

    // Get a product including its features by ID
    public async Task<GenericResponseDto<ProductWithFeaturesDTO>> GetProductWithFeaturesAsync(int id)
    {
        var product = await _repo.ProductRepository.FindAsync(
            p => p.Id == id,
            new string[] {
                "Features",
                "Features.FeatureValues",
                "Images",
                "User"
            }
        );

        if (product == null)
            throw new NotFoundException($"Product with ID {id} not found");

        var productDto = _mapper.Map<ProductWithFeaturesDTO>(product);

        return new GenericResponseDto<ProductWithFeaturesDTO>
        {
            Data = productDto,
            Message = "Product retrieved successfully",
            Status = 200,
            RedirectUrl = null
        };
    }

}
