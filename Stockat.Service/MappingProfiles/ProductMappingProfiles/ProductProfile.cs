using AutoMapper;
using Stockat.Core.DTOs.FeatureDtos;
using Stockat.Core.DTOs.FeatureValueDto;
using Stockat.Core.DTOs.ProductDTOs;
using Stockat.Core.DTOs.ProductImageDto;
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
        CreateMap<Product, ProductHomeDto>()
            .ForMember(dest => dest.Images, src => src.MapFrom(src => src.Images.Select(s => s.ImageUrl)))
            .ReverseMap();

        CreateMap<Product, ProductDetailsDto>()
            .ForMember(dest => dest.ImagesArr, src => src.MapFrom(src => src.Images.Select(s => s.ImageUrl)))
            .ForMember(dest => dest.SellerName, src => src.MapFrom(src => src.User.UserName))
            .ReverseMap();

        CreateMap<AddProductDto, Product>().ReverseMap(); ;
        CreateMap<AddFeatureDto, Feature>().ReverseMap(); ;
        CreateMap<AddFeatureValuesDto, FeatureValue>().ReverseMap(); ;
        CreateMap<AddProductmageDto, ProductImage>().ReverseMap(); ;

        CreateMap<UpdateProductDto, Product>().ReverseMap(); ;
        //CreateMap<AddFeatureDto, Feature>().ReverseMap(); ;
        //CreateMap<AddFeatureValuesDto, FeatureValue>().ReverseMap(); ;
        //CreateMap<AddProductmageDto, ProductImage>().ReverseMap(); ;


    }
}
