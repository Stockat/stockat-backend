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
            CreateMap<AddStockDTO, Stock>().ReverseMap();
            CreateMap<StockDetailsDTO, StockDetails>().ReverseMap();        }
    }
}
