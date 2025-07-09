using AutoMapper;
using Stockat.Core.DTOs.ServiceDTOs;
using Stockat.Core.Entities;

namespace Stockat.Service.MappingProfiles;

public class ServiceEditRequestProfiles : Profile
{
    public ServiceEditRequestProfiles()
    {
        CreateMap<ServiceEditRequest, ServiceEditRequestDto>()
            .ReverseMap();

        CreateMap<ServiceEditRequest, Stockat.Core.Entities.Service>()
        .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.EditedName))
        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.EditedDescription))
        .ForMember(dest => dest.MinQuantity, opt => opt.MapFrom(src => src.EditedMinQuantity))
        .ForMember(dest => dest.PricePerProduct, opt => opt.MapFrom(src => src.EditedPricePerProduct))
        .ForMember(dest => dest.EstimatedTime, opt => opt.MapFrom(src => src.EditedEstimatedTime))
        .ForMember(dest => dest.ImageId, opt => opt.MapFrom(src => src.EditedImageId))
        .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.EditedImageUrl));
    }
}
