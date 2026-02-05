using AutoMapper;

namespace InvestLens.Web.Profiles;

public class GlobalDictionariesProfile : Profile
{
    public GlobalDictionariesProfile()
    {
        CreateMap<Grpc.Service.Engine, Data.Entities.Index.Engine>();
        CreateMap<Grpc.Service.Market, Data.Entities.Index.Market>();
        CreateMap<Grpc.Service.Board, Data.Entities.Index.Board>();
        CreateMap<Grpc.Service.BoardGroup, Data.Entities.Index.BoardGroup>();
        CreateMap<Grpc.Service.Duration, Data.Entities.Index.Duration>();
        CreateMap<Grpc.Service.SecurityType, Data.Entities.Index.SecurityType>();
        CreateMap<Grpc.Service.SecurityGroup, Data.Entities.Index.SecurityGroup>();
        CreateMap<Grpc.Service.SecurityCollection, Data.Entities.Index.SecurityCollection>();
    }
}
