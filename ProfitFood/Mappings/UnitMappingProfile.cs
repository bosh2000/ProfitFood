using AutoMapper;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Domain.ModelsViewModels;

namespace ProfitFood.UI.Mappings
{
    public class UnitMappingProfile : Profile
    {
        public UnitMappingProfile()
        {
            CreateMap<UnitEditModel, UnitItemDto>().ReverseMap();
            CreateMap<UnitEditDto, UnitEditModel>().ReverseMap();
            CreateMap<UnitItemDto, UnitListItemViewModel>().ReverseMap();
            CreateMap<UnitItemDto, Unit>().ReverseMap();
        }
    }
}