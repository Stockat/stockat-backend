using AutoMapper;
using Stockat.Core.DTOs.ProductDTOs;
using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service.MappingProfiles.ProductMappingProfiles;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductHomeDto>().ReverseMap();
        CreateMap<Product, ProductDetailsDto>()
            .ForMember(dest => dest.ImagesArr, src => src.MapFrom(src => src.Images.Select(s => s.ImageUrl)))
            .ForMember(dest => dest.SellerName, src => src.MapFrom(src => src.User.UserName))
            .ReverseMap();


    }
}
