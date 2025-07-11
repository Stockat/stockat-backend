using Stockat.Core.DTOs.CategoryDtos;
using Stockat.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.DTOs.TagsDtos;

namespace Stockat.Core.IServices;

public interface ITagService
{
    Task<GenericResponseDto<IEnumerable<TagDto>>> getAllTags();

    public Task<GenericResponseDto<IEnumerable<TagDto>>> getAllActiveTags();
    public Task<GenericResponseDto<TagDto>> AddTag(string categoryName);
    public Task<GenericResponseDto<TagDto>> EditTag(int id, UpdateTagDto updateTagDto);

    public Task<GenericResponseDto<TagDto>> ChangeTagStatus(int id, TagStatus status);
}
