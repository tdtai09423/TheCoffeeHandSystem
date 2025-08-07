using AutoMapper;
using Domain.Entities;
using Services.DTOs;

namespace Services.MapperProfiles
{
    public class OrderDetailProfile : Profile
    {
        public OrderDetailProfile()
        {
            CreateMap<OrderDetail, OrderDetailResponselDTO>().ReverseMap();
            CreateMap<Drink, DrinkDTO>();
            CreateMap<OrderDetailRequestDTO, OrderDetail>().ReverseMap();
        }
    }
}
