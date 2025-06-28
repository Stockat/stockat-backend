using AutoMapper;
using Stockat.Core.DTOs.ServiceRequestUpdateDTOs;
using Stockat.Core.Entities;

namespace Stockat.Service.MappingProfiles.ServiceMappingProfiles;

public class ServiceRequestUpdateMappingProfile : Profile
{
    public ServiceRequestUpdateMappingProfile()
    {
        CreateMap<ServiceRequestUpdate, ServiceRequestUpdateDto>().ReverseMap();
        CreateMap<CreateServiceRequestUpdateDto, ServiceRequestUpdate>();
    }
   
}
