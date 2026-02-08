using AutoMapper;
using InvestLens.Shared.Models.Dictionaries;

namespace InvestLens.Data.Api.Profiles;

public class GlobalDictionariesProfile : Profile
{
    public GlobalDictionariesProfile()
    {
        CreateMap<Grpc.Service.Engine, EngineModel>();
        CreateMap<Grpc.Service.Market, MarketModel>();
        CreateMap<Grpc.Service.Board, BoardModel>();
        CreateMap<Grpc.Service.BoardGroup, BoardGroupModel>();
        CreateMap<Grpc.Service.Duration, DurationModel>();
        CreateMap<Grpc.Service.SecurityType, SecurityTypeModel>();
        CreateMap<Grpc.Service.SecurityGroup, SecurityGroupModel>();
        CreateMap<Grpc.Service.SecurityCollection, SecurityCollectionModel>();
    }
}
