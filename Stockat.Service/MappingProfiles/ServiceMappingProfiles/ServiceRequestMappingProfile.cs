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
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Service.Seller.FirstName + " " + src.Service.Seller.LastName))
            .ForMember(dest => dest.SelledId, opt => opt.MapFrom(src => src.Service.SellerId))
            .ReverseMap();

        CreateMap<CreateServiceRequestDto, ServiceRequest>()
            .ReverseMap();
    }
}
