using AutoMapper;
using InvestLens.Data.Entities;
using InvestLens.Shared.Models;

namespace InvestLens.Data.Api.Profiles;

public class SecuritiesProfile : Profile
{
    public SecuritiesProfile()
    {
        CreateMap<Grpc.Service.Security, SecurityModel>().ReverseMap();
        CreateMap<SecurityEntity, SecurityWithDetailsModel>();
    }
}
