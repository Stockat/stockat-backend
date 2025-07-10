using AutoMapper;
using Stockat.Core.DTOs.ReviewDTOs;
using Stockat.Core.Entities;

namespace Stockat.Service.MappingProfiles.ReviewMappingProfiles
{
    public class ReviewProfile : Profile
    {
        public ReviewProfile()
        {
            CreateMap<CreateReviewDto, Review>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewerId, opt => opt.Ignore())
                .ForMember(dest => dest.Reviewer, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Service, opt => opt.Ignore())
                .ForMember(dest => dest.OrderProduct, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceRequest, opt => opt.Ignore());

            CreateMap<UpdateReviewDto, Review>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewerId, opt => opt.Ignore())
                .ForMember(dest => dest.Reviewer, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceId, opt => opt.Ignore())
                .ForMember(dest => dest.OrderProductId, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceRequestId, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Service, opt => opt.Ignore())
                .ForMember(dest => dest.OrderProduct, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceRequest, opt => opt.Ignore());

            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.ReviewerName, opt => opt.MapFrom(src => src.Reviewer != null ? src.Reviewer.UserName : string.Empty))
                .ForMember(dest => dest.ReviewerEmail, opt => opt.MapFrom(src => src.Reviewer != null ? src.Reviewer.Email : string.Empty))
                .ForMember(dest => dest.ReviewerImageUrl, opt => opt.MapFrom(src => src.Reviewer != null ? src.Reviewer.ProfileImageUrl : null))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service != null ? src.Service.Name : null));
        }
    }
} 