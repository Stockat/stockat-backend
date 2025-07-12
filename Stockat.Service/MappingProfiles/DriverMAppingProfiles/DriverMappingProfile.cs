using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Stockat.Core.DTOs.DriverDTOs;
using Stockat.Core.Entities;

namespace Stockat.Service.MappingProfiles.DriverMAppingProfile
{
    public class DriverMappingProfile : Profile
    {
        public DriverMappingProfile()
        {
            CreateMap<DriverCreateDto, Driver>();
            CreateMap<DriverUpdateDto, Driver>();
            CreateMap<Driver, DriverDTO>();
        }
    }
}
