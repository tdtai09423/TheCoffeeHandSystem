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
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
                .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.ImageUrl))
                .ForMember(d => d.Price, opt => opt.MapFrom(s => s.Price))
                .ForMember(d => d.CategoryId, opt => opt.MapFrom(s => s.CategoryId))
                .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Category))
                .ForMember(d => d.Recipe, opt => opt.MapFrom(s => s.Recipes))
                .ReverseMap();
            CreateMap<DrinkRequestDTO, Drink>()
               .ReverseMap();
        }
    }
}
