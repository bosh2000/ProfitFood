using AutoMapper;
using ProfitFood.Model.DBModel;
using ProfitFood.UI.Models.View;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
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

            CreateMap<BaseUnitStorage, BaseUnitStorageItemView>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            CreateMap<BaseUnitStorageItemView, BaseUnitStorage>()
                .ConstructUsing(src => BaseUnitStorage.Create(src.Name, src.Description).Value)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());
        }
    }
}