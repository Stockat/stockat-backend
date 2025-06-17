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
                //.ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                //.ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                //.ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                //.ForMember(dest => dest.CurrentBid, opt => opt.MapFrom(src => src.StartingPrice)) // Initialize with starting price
                //.ForMember(dest => dest.StartingPrice, opt => opt.MapFrom(src => src.StartingPrice))
                //.ForMember(dest => dest.IncrementUnit, opt => opt.MapFrom(src => src.IncrementUnit))
                //.ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                //.ForMember(dest => dest.StockId, opt => opt.MapFrom(src => src.StockId))
                //.ForMember(dest => dest.SellerId, opt => opt.MapFrom(src => src.SellerId))
                //.ForMember(dest => dest.IsClosed, opt => opt.MapFrom(_ => false)) // New auctions are always open
                //.ForMember(dest => dest.BuyerId, opt => opt.Ignore()) // Not set during creation
                //.ForMember(dest => dest.BuyerUser, opt => opt.Ignore())
                //.ForMember(dest => dest.AuctionBidRequest, opt => opt.Ignore())
                //.ForMember(dest => dest.AuctionOrder, opt => opt.Ignore())
                //.ForMember(dest => dest.Product, opt => opt.Ignore()) // Entity, not set from DTO
                //.ForMember(dest => dest.Stock, opt => opt.Ignore())   // Entity, not set from DTO
                //.ForMember(dest => dest.SellerUser, opt => opt.Ignore());

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
