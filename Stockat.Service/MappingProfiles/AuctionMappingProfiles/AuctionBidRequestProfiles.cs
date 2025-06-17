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
 
    public class AuctionBidRequestProfiles : Profile
    {
        public AuctionBidRequestProfiles()
        {
            CreateMap<AuctionBidRequestCreateDto, AuctionBidRequest>();
            CreateMap<AuctionBidRequest, AuctionBidRequestDto>();
        }
    }
}
