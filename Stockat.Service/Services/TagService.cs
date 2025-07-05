using AutoMapper;
using Stockat.Core.IServices;
using Stockat.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.DTOs.CategoryDtos;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.TagsDtos;

namespace Stockat.Service.Services;

public class TagService : ITagService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repo;
    public TagService(ILoggerManager logger, IMapper mapper, IRepositoryManager repo)
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
    }


    public async Task<GenericResponseDto<IEnumerable<TagDto>>> getAllTags()
    {
        var res = await _repo.TagRepo.GetAllAsync();

        var mappedResult = _mapper.Map<IEnumerable<TagDto>>(res);

        return new GenericResponseDto<IEnumerable<TagDto>>()
        {
            Data = mappedResult,
            Status = 200,
            Message = "Success",
            RedirectUrl = null,
        };

    }
}
