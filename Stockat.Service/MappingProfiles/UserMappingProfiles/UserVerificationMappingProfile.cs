using AutoMapper;
using Stockat.Core.DTOs.UserVerificationDTOs;
using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service.MappingProfiles.UserMappingProfiles;

public class UserVerificationProfile : Profile
{
    public UserVerificationProfile()
    {
        // map from entity to readdto
        CreateMap<UserVerification, UserVerificationReadDto>();

    }
}
