using AutoMapper;
using InvestLens.Shared.Models;

namespace InvestLens.Web.Profiles;

public class SecuritiesProfile : Profile
{
    public SecuritiesProfile()
    {
        CreateMap<Grpc.Service.Security, SecurityModel>();
    }
}
