using AutoMapper;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.CategoryDtos;
using Stockat.Core.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service.Services;

public class CategoryService : ICategoryService
{

    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repo;
    public CategoryService(ILoggerManager logger, IMapper mapper, IRepositoryManager repo)
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
    }
    public async Task<GenericResponseDto<IEnumerable<CategoryDto>>> getAllCategory()
    {
        var res = await _repo.CategoryRepo.GetAllAsync();

        var mappedResult = _mapper.Map<IEnumerable<CategoryDto>>(res);

        return new GenericResponseDto<IEnumerable<CategoryDto>>()
        {
            Data = mappedResult,
            Status = 200,
            Message = "Success",
            RedirectUrl = null,
        };

    }
}
