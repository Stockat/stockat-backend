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
using Stockat.Core.Entities;

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

    public async Task<GenericResponseDto<IEnumerable<TagDto>>> getAllActiveTags()
    {
        var res = await _repo.TagRepo.FindAllAsync(t => t.Status == TagStatus.Activated);

        var mappedResult = _mapper.Map<IEnumerable<TagDto>>(res);

        return new GenericResponseDto<IEnumerable<TagDto>>()
        {
            Data = mappedResult,
            Status = 200,
            Message = "Tags Fetched Successfully",
            RedirectUrl = null,
        };

    }


    public async Task<GenericResponseDto<TagDto>> AddTag(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            return new GenericResponseDto<TagDto>
            {
                Status = 400,
                Message = "Tag Name  is Required."
            };
        var tag = new Tag() { Name = tagName, Status = TagStatus.Activated };
        await _repo.TagRepo.AddAsync(tag);
        await _repo.CompleteAsync();

        var tagDto = _mapper.Map<TagDto>(tag);

        return new GenericResponseDto<TagDto>()
        {
            Data = tagDto,
            Status = 201,
            RedirectUrl = null,
            Message = $"Tag Created Successfully {tagName}"
        };

    }
    public async Task<GenericResponseDto<TagDto>> EditTag(int id, UpdateTagDto updateTagDto)
    {
        if (id == 0 || updateTagDto is null)
        {
            return new GenericResponseDto<TagDto>
            {
                Status = 400,
                Message = "TagDto & Id Required."
            };
        }

        var oldTag = await _repo.TagRepo.FindAsync(c => c.Id == id && c.Status == TagStatus.Activated);

        if (oldTag is null)
        {
            return new GenericResponseDto<TagDto>
            {
                Status = 404,
                Message = "Tag Not Found."
            };
        }

        oldTag.Name = updateTagDto.TagName;

        var updatedTagDto = _mapper.Map<TagDto>(oldTag);

        await _repo.CompleteAsync();

        return new GenericResponseDto<TagDto>
        {
            Data = updatedTagDto,
            Status = 200,
            Message = "Tag Updated Successfully."
        };

    }
    public async Task<GenericResponseDto<TagDto>> ChangeTagStatus(int id, TagStatus status)
    {
        if (id == 0)
        {
            return new GenericResponseDto<TagDto>
            {
                Status = 400,
                Message = "Status & Id Required."
            };
        }

        var oldTag = await _repo.TagRepo.FindAsync(c => c.Id == id);

        if (oldTag is null)
        {
            return new GenericResponseDto<TagDto>
            {
                Status = 404,
                Message = "Tag Not Found."
            };
        }

        oldTag.Status = status;

        var updatedTagDto = _mapper.Map<TagDto>(oldTag);

        await _repo.CompleteAsync();

        return new GenericResponseDto<TagDto>
        {
            Data = updatedTagDto,
            Status = 200,
            Message = "Tag Updated Successfully."
        };

    }

}
