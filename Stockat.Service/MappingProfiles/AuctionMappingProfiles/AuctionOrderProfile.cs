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
             .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AuctionRequest.BidAmount));

        }
    }
}
