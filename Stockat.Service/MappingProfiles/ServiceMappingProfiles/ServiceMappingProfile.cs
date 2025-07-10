using AutoMapper;
using Stockat.Core.DTOs.ServiceDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using System;
using System.Linq;

namespace Stockat.Service.MappingProfiles.ServiceMappingProfiles;

public class ServiceMappingProfile : Profile
{
    public ServiceMappingProfile() {
        CreateMap<CreateServiceDto, Stockat.Core.Entities.Service>();
        CreateMap<Stockat.Core.Entities.Service, ServiceDto>()
            .ForMember(dest => dest.SellerName,
                       opt => opt.MapFrom(src => src.Seller.FirstName + " " + src.Seller.LastName))
            .ForMember(dest => dest.SellerIsDeleted,
                       opt => opt.MapFrom(src => src.Seller.IsDeleted))
            .ForMember(dest => dest.SellerIsBlocked,
                        opt => opt.MapFrom(src =>
                            src.Seller.Punishments != null &&
                            src.Seller.Punishments.Any(p =>
                                (p.Type == PunishmentType.TemporaryBan || p.Type == PunishmentType.PermanentBan) &&
                                (p.EndDate == null || p.EndDate > DateTime.UtcNow)
                            )
                        )
            );

        CreateMap<UpdateServiceDto, Stockat.Core.Entities.Service>()
            .ForMember(dest => dest.Name, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Name)))
            .ForMember(dest => dest.Description, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Description)))
            .ForMember(dest => dest.MinQuantity, opt => opt.Condition(src => src.MinQuantity.HasValue))
            .ForMember(dest => dest.PricePerProduct, opt => opt.Condition(src => src.PricePerProduct.HasValue))
            .ForMember(dest => dest.EstimatedTime, opt => opt.Condition(src => !string.IsNullOrEmpty(src.EstimatedTime)))
            .ForMember(dest => dest.ImageId, opt => opt.Condition(src => !string.IsNullOrEmpty(src.ImageId)))
            .ForMember(dest => dest.ImageUrl, opt => opt.Condition(src => !string.IsNullOrEmpty(src.ImageUrl)));
    }
}
