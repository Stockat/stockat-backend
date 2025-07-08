using AutoMapper;
using Stockat.Core.Entities;
using Stockat.Core.DTOs.UserPunishmentDTOs;

namespace Stockat.Service.MappingProfiles.UserMappingProfiles
{
    public class PunishmentMappingProfile : Profile
    {
        public PunishmentMappingProfile()
        {
            CreateMap<UserPunishment, PunishmentReadDto>();
        }
    }
} 