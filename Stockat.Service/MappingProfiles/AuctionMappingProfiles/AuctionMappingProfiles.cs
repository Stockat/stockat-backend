using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service.MappingProfiles.AuctionMappingProfiles
{
    using AutoMapper;
    using Stockat.Core.DTOs.AuctionDTOs;
    using Stockat.Core.Entities;

    public class AuctionMappingProfile : Profile
    {
        public AuctionMappingProfile()
        {
            CreateMap<Auction, AuctionDetailsDto>();
                
            CreateMap<AuctionDetailsDto, Auction>();

            // AuctionCreateDto -> Auction
            CreateMap<AuctionCreateDto, Auction>();

            // Auction -> AuctionCreateDto
            CreateMap<Auction, AuctionCreateDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.SellerId, opt => opt.MapFrom(src => src.SellerId));
            
            CreateMap<Auction, AuctionUpdateDto>();
            CreateMap<AuctionUpdateDto, Auction>();

        }
    }
}
