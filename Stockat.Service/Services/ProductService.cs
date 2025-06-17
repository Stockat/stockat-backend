using AutoMapper;
using Stockat.Core;
using Stockat.Core.Consts;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.ProductDTOs;
using Stockat.Core.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


    public async Task<GenericResponseDto<PaginatedDto<ProductHomeDto>>> getAllProductsPaginated()
    {
        var res = await _repo.ProductRepository.FindAllAsync(p => p.isDeleted == false, 0, 9, o => o.Id, OrderBy.Descending);

        var productDtos = _mapper.Map<ProductHomeDto>(res);

        var paginatedres = new PaginatedDto<ProductHomeDto>()
        {

            PaginatedData = productDtos,
            Size = 18,
            Count = 9,
            Page = 1
        };



        var resDto = new GenericResponseDto<PaginatedDto<ProductHomeDto>>()
        {
            Data = paginatedres,
            Message = "Success",
            Status = 200,
            RedirectUrl = null,
        };

        return resDto;
    }
}
