using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Stockat.Core.DTOs.StockDTOs;
using Stockat.Core.Entities;

namespace Stockat.Service.MappingProfiles.StockMappingProfiles
{
    public class AddStockMappingProfile : Profile
    {
        public AddStockMappingProfile()
        {
            CreateMap<AddStockDTO, Stock>()
                .ForMember(dest => dest.StockDetails, 
                           opt => opt.MapFrom(src => src.StockDetails))
                .ReverseMap();
                
            CreateMap<StockDetailsDTO, StockDetails>()
                .ReverseMap();

            CreateMap<Stock, StockDTO>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.StockFeatures, opt => opt.MapFrom(src => src.StockDetails));

            CreateMap<StockDetails, StockFeaturesDTO>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Feature.Name))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.FeatureValue.Value))
                .ReverseMap();
        }
    }
}
