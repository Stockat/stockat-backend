using AutoMapper;
using Stockat.Core.DTOs.CategoryDtos;
using Stockat.Core.DTOs.ProductDTOs;
using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service.MappingProfiles.CategoryMappingProfiles;

public class TagProfile : Profile
{
    public TagProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ReverseMap();
    }
}
