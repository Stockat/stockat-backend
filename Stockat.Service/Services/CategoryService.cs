using AutoMapper;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.CategoryDtos;
using Stockat.Core.DTOs.OrderDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Exceptions;
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
    public async Task<GenericResponseDto<IEnumerable<CategoryDto>>> getAllActiveCategory()
    {
        var res = await _repo.CategoryRepo.FindAllAsync(c => c.IsDeleted == false);

        var mappedResult = _mapper.Map<IEnumerable<CategoryDto>>(res);

        return new GenericResponseDto<IEnumerable<CategoryDto>>()
        {
            Data = mappedResult,
            Status = 200,
            Message = "Success",
            RedirectUrl = null,
        };

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


    public async Task<GenericResponseDto<CategoryDto>> AddCategory(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return new GenericResponseDto<CategoryDto>
            {
                Status = 400,
                Message = "Category name is Required."
            };
        var category = new Category() { CategoryName = categoryName };
        await _repo.CategoryRepo.AddAsync(category);
        await _repo.CompleteAsync();

        var categoryDto = _mapper.Map<CategoryDto>(category);

        return new GenericResponseDto<CategoryDto>()
        {
            Data = categoryDto,
            Status = 201,
            RedirectUrl = null,
            Message = $"Category Created Successfully{categoryName}"
        };

    }
    public async Task<GenericResponseDto<CategoryDto>> EditCategory(int id, CategoryDto categoryDto)
    {
        if (id == 0 || categoryDto is null)
        {
            return new GenericResponseDto<CategoryDto>
            {
                Status = 400,
                Message = "CategoryDto & Id Required."
            };
        }

        var oldCategory = await _repo.CategoryRepo.FindAsync(c => c.Id == id && c.IsDeleted == false);

        if (oldCategory is null)
        {
            return new GenericResponseDto<CategoryDto>
            {
                Status = 404,
                Message = "Category Not Found."
            };
        }

        oldCategory.CategoryName = categoryDto.CategoryName;

        var updatedCategoryDto = _mapper.Map<CategoryDto>(oldCategory);

        await _repo.CompleteAsync();

        return new GenericResponseDto<CategoryDto>
        {
            Data = updatedCategoryDto,
            Status = 200,
            Message = "Category Updated Successfully."
        };

    }
    public async Task<GenericResponseDto<CategoryDto>> DeleteCategory(int id)
    {
        if (id == 0)
        {
            return new GenericResponseDto<CategoryDto>
            {
                Status = 400,
                Message = "CategoryDto & Id Required."
            };
        }

        var oldCategory = await _repo.CategoryRepo.FindAsync(c => c.Id == id && c.IsDeleted == false, ["Products"]);

        if (oldCategory is null)
        {
            return new GenericResponseDto<CategoryDto>
            {
                Status = 404,
                Message = "Category Not Found."
            };
        }

        if (oldCategory.Products.Count > 0)
            return new GenericResponseDto<CategoryDto>
            {
                Status = 400,
                Message = "Category Can't be deleted as it Has Products Assigned To it"
            };

        oldCategory.IsDeleted = true;

        var updatedCategoryDto = _mapper.Map<CategoryDto>(oldCategory);

        await _repo.CompleteAsync();

        return new GenericResponseDto<CategoryDto>
        {
            Data = updatedCategoryDto,
            Status = 200,
            Message = "Category Deleted Successfully."
        };

    }


}
