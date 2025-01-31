

using AutoMapper;
using AmCart.ProductSearch.API.Entities;

namespace AmCart.ProductSearch.API.AutoMapper
{
    /// <summary>
    /// AutoMapper profile to map MongoDB Product entity to Elasticsearch ProductSearch entity.
    /// </summary>
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<Product, AmCart.ProductSearch.API.Entities.ProductSearch>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString())) // Convert int to string
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id)) // Store int separately
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating));

            CreateMap<ProductRating, ProductSearchRating>()
                .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => src.Rate))
                .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count));
        }
    }
}



