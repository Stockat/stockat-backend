using AutoMapper;
using Stockat.Core.DTOs.FeatureDtos;
using Stockat.Core.DTOs.FeatureValueDto;
using Stockat.Core.DTOs.ProductDTOs;
using Stockat.Core.DTOs.ProductImageDto;
using Stockat.Core.DTOs.TagsDtos;
using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Stockat.Service.MappingProfiles.ProductMappingProfiles;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductHomeDto>()
            .ForMember(dest => dest.Images, src => src.MapFrom(src => src.Images.Select(s => s.ImageUrl)))
            .ForMember(dest => dest.CategoryName, src => src.MapFrom(src => src.Category.CategoryName))
            .ReverseMap();

        CreateMap<Product, ProductDetailsDto>()
            .ForMember(dest => dest.ImagesArr, src => src.MapFrom(src => src.Images.Select(s => s.ImageUrl)))
            .ForMember(dest => dest.CategoryName, src => src.MapFrom(src => src.Category.CategoryName))
            .ForMember(dest => dest.SellerName, src => src.MapFrom(src => src.User.UserName))
            .ReverseMap();

        // Add Product From Mapping 
        CreateMap<AddProductDto, Product>().ReverseMap();
        CreateMap<AddFeatureDto, Feature>().ReverseMap();
        CreateMap<AddTagDto, ProductTag>().ReverseMap();
        CreateMap<AddFeatureValuesDto, FeatureValue>()
            .ForMember(dest => dest.Value, src => src.MapFrom(src => src.Name))
            .ReverseMap();
        CreateMap<AddProductmageDto, ProductImage>().ReverseMap();


        // Update Product From Mapping 
        CreateMap<UpdateProductDto, Product>()
            .ReverseMap();
        CreateMap<UpdateFeatureDto, Feature>().ReverseMap();
        CreateMap<UpdateTagDto, ProductTag>().ReverseMap();
        CreateMap<UpdateFeatureValueDto, FeatureValue>()
            .ForMember(dest => dest.Value, src => src.MapFrom(src => src.Name))
            .ReverseMap();
        CreateMap<UpdateProductImageDto, ProductImage>().ReverseMap();


        // View Seller Product
        CreateMap<Product, GetSellerProductDto>()
                 .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Images.Select(img => img.ImageUrl)));

        CreateMap<Product, ProductWithFeaturesDTO>()
            .ForMember(dest => dest.Images,
                opt => opt.MapFrom(src => src.Images.Select(i => i.ImageUrl)))
            .ForMember(dest => dest.SellerName,
                opt => opt.MapFrom(src => src.User.UserName))
            .ForMember(dest => dest.Features,
                opt => opt.MapFrom(src => src.Features))
            .ReverseMap();

        CreateMap<Feature, FeatureWithValuesDTO>()
            .ForMember(dest => dest.Values,
                opt => opt.MapFrom(src => src.FeatureValues));

        CreateMap<FeatureValue, FeatureValueDTO>();

        CreateMap<Product, ProductWithFeaturesDTO>()
           .ForMember(dest => dest.Images,
               opt => opt.MapFrom(src => src.Images.Select(i => i.ImageUrl)))
           .ForMember(dest => dest.SellerName,
               opt => opt.MapFrom(src => src.User.UserName))
           .ForMember(dest => dest.Features,
               opt => opt.MapFrom(src => src.Features))
           .ReverseMap();

        CreateMap<Feature, FeatureWithValuesDTO>()
            .ForMember(dest => dest.Values,
                opt => opt.MapFrom(src => src.FeatureValues));

    }
}
