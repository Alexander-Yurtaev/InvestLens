using AutoMapper;
using InvestLens.Shared.Models;

namespace InvestLens.Gateway.Profiles;

public class SecuritiesProfile : Profile
{
    public SecuritiesProfile()
    {
        CreateMap<SecurityWithDetailsModel, Grpc.Service.SecurityWithDetails>().ReverseMap();

        CreateMap<Grpc.Service.GetSecuritiesWithDetailsResponse, SecurityWithDetailsModelWithPagination>()
            .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.Models, opt => opt.MapFrom(src => src.Data))
            ;
    }
}