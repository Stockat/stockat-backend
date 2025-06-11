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
        }
    }
}
