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


    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<ProductHomeDto>>>> getAllProductsPaginated(int _size, int _page)

    {
        int skip = (_page - 1) * _size;
        int take = _size;

        var res = await _repo.ProductRepository.FindAllAsync(p => p.isDeleted == false, skip, take, o => o.Id, OrderBy.Descending);

        res.TryGetNonEnumeratedCount(out var count);

        var productDtos = _mapper.Map<IEnumerable<ProductHomeDto>>(res);

        var paginatedres = new PaginatedDto<IEnumerable<ProductHomeDto>>()
        {

            PaginatedData = productDtos,
            Size = count,
            Count = 9,
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


}
