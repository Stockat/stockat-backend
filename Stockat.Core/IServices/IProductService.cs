using Microsoft.AspNetCore.Http;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.MediaDTOs;
using Stockat.Core.DTOs.ProductDTOs;
using Stockat.Core.DTOs.UserDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.IServices;

public interface IProductService
{
    Task<GenericResponseDto<PaginatedDto<IEnumerable<ProductHomeDto>>>> getAllProductsPaginated
        (int _size, int _page, string location, int category, int minQuantity, int minPrice, int[] tags);
    Task<GenericResponseDto<PaginatedDto<IEnumerable<ProductHomeDto>>>> getAllProductsPaginatedForAdmin
     (int _size, int _page, string location, int category, int minQuantity, int minPrice, int[] tags);



    Task<GenericResponseDto<ProductDetailsDto>> GetProductDetailsAsync(int id);
    Task<GenericResponseDto<UpdateProductDto>> GetProductForUpdateAsync(int id);
    Task<GenericResponseDto<PaginatedDto<IEnumerable<GetSellerProductDto>>>> GetAllProductForSellerAsync
        (int _size, int _page, string location, int category, int minQuantity, int minPrice, int[] tags);
    public Task<int> AddProductAsync(AddProductDto productDto);
    public Task<int> UpdateProduct(int id, UpdateProductDto productDto);
    public Task<GenericResponseDto<string>> ChangeProductStatus(int id, ProductStatus chosenStatus);
    public Task<GenericResponseDto<string>> RemoveProduct(int id);
    public Task<GenericResponseDto<string>> ChangeCanBeRequested(int id);

    public Task<GenericResponseDto<IEnumerable<ImageUploadResultDto>>> UploadProductImages(IFormFile[] imgs);

    public Task<GenericResponseDto<ProductWithFeaturesDTO>> GetProductWithFeaturesAsync(int id);
}
