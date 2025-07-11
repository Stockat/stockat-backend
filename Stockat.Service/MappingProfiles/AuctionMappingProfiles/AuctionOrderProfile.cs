using AutoMapper;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service.MappingProfiles.AuctionMappingProfiles
{
    public class AuctionOrderProfile : Profile
    {
        public AuctionOrderProfile()
        {
            CreateMap<AuctionOrder, AuctionOrderDto>()
             .ForMember(dest => dest.WinningBidId, opt => opt.MapFrom(src => src.AuctionRequestId))
             .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AuctionRequest.BidAmount))
             .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.ShippingAddress))
             .ForMember(dest => dest.RecipientName, opt => opt.MapFrom(src => src.RecipientName))
             .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
             .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));

            CreateMap<AuctionOrderDto, AuctionOrder>()
             .ForMember(dest => dest.AuctionRequestId, opt => opt.MapFrom(src => src.WinningBidId))
             .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.ShippingAddress))
             .ForMember(dest => dest.RecipientName, opt => opt.MapFrom(src => src.RecipientName))
             .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
             .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));
        }
    }
}
