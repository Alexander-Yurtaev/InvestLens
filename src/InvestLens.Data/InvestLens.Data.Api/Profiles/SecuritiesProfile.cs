using AutoMapper;
using InvestLens.Data.Entities;
using InvestLens.Grpc.Service;
using InvestLens.Shared.Models;

namespace InvestLens.Data.Api.Profiles;

public class SecuritiesProfile : Profile
{
    public SecuritiesProfile()
    {
        CreateMap<SecurityModel, Grpc.Service.Security>();
        CreateMap<SecurityModelWithPagination, Grpc.Service.GetSecuritiesResponse>();
        CreateMap<SecurityEntity, SecurityWithDetailsModel>();
        CreateMap<SecurityModel, SecurityWithDetails>();
        CreateMap<SecurityWithDetailsModelWithPagination, Grpc.Service.GetSecuritiesWithDetailsResponse>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Models));
        CreateMap<EntitiesWithPagination<SecurityEntity>, SecurityModelWithPagination>();
    }
}