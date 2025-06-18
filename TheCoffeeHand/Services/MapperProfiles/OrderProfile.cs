using AutoMapper;
using Domain.Entities;
using Services.DTOs;

namespace Services.MapperProfiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderResponseDTO>()
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails))
                .ReverseMap();
            CreateMap<OrderRequestDTO, Order>()
                .ReverseMap();

        }
    }
}
