using AutoMapper;
using Stockat.Core.DTOs.ServiceRequestDTOs;
using Stockat.Core.Entities;

namespace Stockat.Service.MappingProfiles.ServiceMappingProfiles;

public class ServiceRequestMappingProfile : Profile
{
    public ServiceRequestMappingProfile()
    {
        CreateMap<ServiceRequest, ServiceRequestDto>()
            .ForMember(dest => dest.BuyerName, opt => opt.MapFrom(src => src.Buyer.FirstName + " " + src.Buyer.LastName))
            .ForMember(dest => dest.ServiceTitle, opt => opt.MapFrom(src => src.Service.Name))
            .ReverseMap();

        CreateMap<CreateServiceRequestDto, ServiceRequest>()
            .ReverseMap();
    }
}
