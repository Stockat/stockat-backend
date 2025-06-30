using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using Stockat.Core;
using Stockat.Core.Consts;
using Stockat.Core.DTOs;
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
    private readonly IRepositoryManager _repo;
    public ProductService(ILoggerManager logger, IMapper mapper, IRepositoryManager repo)
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
    }



    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<ProductHomeDto>>>> getAllProductsPaginated
        (int _size, int _page, string location, string category, int minQuantity, int minPrice, string[] tags)

    {
        int skip = (_page - 1) * _size;
        int take = _size;

        var res = await _repo.ProductRepository.FindAllAsync
            (
            p => p.isDeleted == false &&
            p.MinQuantity >= minQuantity &&
            p.Price >= minPrice &&
              (
            tags.Length == 0 ||
            p.ProductTags.Any(pt => tags.Contains(pt.Tag.Name))
              ) &&
             (
             string.IsNullOrEmpty(location) ||
             p.Location.ToString().ToUpper() == location.ToUpper()
             ) &&
            (
            string.IsNullOrEmpty(category) ||
             p.Category.CategoryName.ToUpper() == category.ToUpper()
             )
            , skip: skip, take: take, includes: ["Images"], o => o.Id, OrderBy.Descending
            );

        res.TryGetNonEnumeratedCount(out var count);

        var productDtos = _mapper.Map<IEnumerable<ProductHomeDto>>(res);

        var paginatedres = new PaginatedDto<IEnumerable<ProductHomeDto>>()
        {

            PaginatedData = productDtos,
            Size = 4,
            Count = count,
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
            p => p.Id == id && p.isDeleted == false, ["Images", "Stocks"]

            );

        return new GenericResponseDto<ProductDetailsDto>()
        {
            Data = res,
            Message = "Success",
            Status = 200,
            RedirectUrl = null,
        };

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

        var oldProduct = await _repo.ProductRepository.FindAsync(p => p.Id == id && p.isDeleted == false, ["Images", "Stocks"]);

        if (oldProduct == null)
            throw new NotFoundException($"Product With Id:{id} Not Found, please Contact with Admin for further information");

        _mapper.Map(productDto, oldProduct);

        _repo.ProductRepository.Update(oldProduct);

        return await _repo.CompleteAsync();

    }

    public async Task<int> ChangeProductStatus(int id, ProductStatus chosenStatus)
    {
        var product = await _repo.ProductRepository.FindAsync(p => p.Id == id && p.isDeleted == false);

        if (product == null)
            throw new NotFoundException($"Product With Id:{id} Not Found, please Contact with Admin for further information");

        product.ProductStatus = chosenStatus;

        return await _repo.CompleteAsync();

    }


}
