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
            // SNAPSHOT FIELDS
            .ForMember(dest => dest.ServiceNameSnapshot, opt => opt.MapFrom(src => src.ServiceNameSnapshot))
            .ForMember(dest => dest.ServiceDescriptionSnapshot, opt => opt.MapFrom(src => src.ServiceDescriptionSnapshot))
            .ForMember(dest => dest.ServiceMinQuantitySnapshot, opt => opt.MapFrom(src => src.ServiceMinQuantitySnapshot))
            .ForMember(dest => dest.ServicePricePerProductSnapshot, opt => opt.MapFrom(src => src.ServicePricePerProductSnapshot))
            .ForMember(dest => dest.ServiceEstimatedTimeSnapshot, opt => opt.MapFrom(src => src.ServiceEstimatedTimeSnapshot))
            .ForMember(dest => dest.ServiceImageUrlSnapshot, opt => opt.MapFrom(src => src.ServiceImageUrlSnapshot))
            .ReverseMap();

        CreateMap<CreateServiceRequestDto, ServiceRequest>()
            .ReverseMap();
    }
}
