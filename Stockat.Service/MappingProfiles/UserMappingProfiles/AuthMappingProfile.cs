using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Stockat.Core.DTOs.UserDTOs;
using Stockat.Core.Entities;

namespace Stockat.Service.MappingProfiles.UserMappingProfiles;

public class AuthMappingProfile: Profile
{
    public AuthMappingProfile()
    {
        CreateMap<UserForRegistrationDto, User>();
    }
}
