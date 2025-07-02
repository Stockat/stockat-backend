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
    Task<GenericResponseDto<PaginatedDto<IEnumerable<ProductHomeDto>>>> getAllProductsPaginated(int _size, int _page, string location, string category, int minQuantity, int minPrice, string[] tags);
    Task<GenericResponseDto<ProductDetailsDto>> GetProductDetailsAsync(int id);
    public Task<int> AddProductAsync(AddProductDto productDto);
    public Task<int> UpdateProduct(int id, UpdateProductDto productDto);
    public Task<int> ChangeProductStatus(int id, ProductStatus chosenStatus);

    public Task<GenericResponseDto<IEnumerable<string>>> UploadProductImages(IFormFile[] imgs);
}
