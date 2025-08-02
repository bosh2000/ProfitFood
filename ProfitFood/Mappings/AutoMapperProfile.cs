using AutoMapper;
using ProfitFood.Model.DBModel;
using ProfitFood.UI.Models.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.UI.Mappings
{
    internal class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Entity → DTO/View
            CreateMap<BaseUnit, BaseUnitItemView>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            // DTO/View → Entity
            CreateMap<BaseUnitItemView, BaseUnit>()
                .ConstructUsing(src => BaseUnit.Create(src.Name).Value)
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id генерируется в БД
                .ForMember(dest => dest.Products, opt => opt.Ignore());
        }
    }
}