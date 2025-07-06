using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Stockat.Core.DTOs.OrderDTOs;
using Stockat.Core.Entities;

namespace Stockat.Service.MappingProfiles.OrderMappingProfiles
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile() 
        {
            CreateMap<AddOrderDTO, OrderProduct>()
                .ForMember(dest => dest.OrderType, opt => opt.MapFrom(src => src.OrderType))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.StockId, opt => opt.MapFrom(src => src.StockId))
                .ForMember(dest => dest.SellerId, opt => opt.MapFrom(src => src.SellerId))
                .ForMember(dest => dest.BuyerId, opt => opt.MapFrom(src => src.BuyerId))
                .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.PaymentId))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus))
                .ReverseMap();

            CreateMap<OrderProduct, OrderDTO>()
                .ForMember(dest => dest.SellerFirstName, opt => opt.MapFrom(src => src.Seller.FirstName))
                .ForMember(dest => dest.SellerLastName, opt => opt.MapFrom(src => src.Seller.LastName))
                .ForMember(dest => dest.BuyerFirstName, opt => opt.MapFrom(src => src.Buyer.FirstName))
                .ForMember(dest => dest.BuyerLastName, opt => opt.MapFrom(src => src.Buyer.LastName))
                .ReverseMap();

        }
    }
}
