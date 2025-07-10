using AutoMapper;
using Stockat.Core.DTOs.ServiceDTOs;
using Stockat.Core.Entities;

namespace Stockat.Service.MappingProfiles;

public class ServiceEditRequestProfiles : Profile
{
    public ServiceEditRequestProfiles()
    {
        CreateMap<ServiceEditRequest, ServiceEditRequestDto>()
            .ForMember(dest => dest.IsReactivationRequest, opt => opt.MapFrom(src => src.IsReactivationRequest));
        
        CreateMap<CreateServiceEditRequestDto, ServiceEditRequest>()
            .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => Stockat.Core.Enums.EditApprovalStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<ServiceEditRequest, Stockat.Core.Entities.Service>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.EditedName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.EditedDescription))
            .ForMember(dest => dest.MinQuantity, opt => opt.MapFrom(src => src.EditedMinQuantity))
            .ForMember(dest => dest.PricePerProduct, opt => opt.MapFrom(src => src.EditedPricePerProduct))
            .ForMember(dest => dest.EstimatedTime, opt => opt.MapFrom(src => src.EditedEstimatedTime))
            .ForMember(dest => dest.ImageId, opt => opt.MapFrom(src => src.EditedImageId))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.EditedImageUrl))
            .ForSourceMember(src => src.IsDeferred, opt => opt.DoNotValidate()) // << this avoids mapping back
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // 🔒 Don't try to overwrite the primary key
            .ForMember(dest => dest.SellerId, opt => opt.Ignore()); // optional: ignore seller link too

    }
}
