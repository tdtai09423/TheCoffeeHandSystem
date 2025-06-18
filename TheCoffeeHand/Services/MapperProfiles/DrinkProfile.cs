using AutoMapper;
using Domain.Entities;
using Services.DTOs;

namespace Services.MapperProfiles
{
    public class DrinkProfile : Profile
    {
        public DrinkProfile()
        {
            CreateMap<Drink, DrinkResponseDTO>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Recipe, opt => opt.MapFrom(src => src.Recipes))
                .ReverseMap();
            CreateMap<DrinkRequestDTO, Drink>()
               .ReverseMap();
        }
    }
}
