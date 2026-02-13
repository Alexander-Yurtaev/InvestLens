using AutoMapper;
using InvestLens.Data.Entities;
using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Models.Dictionaries;

namespace InvestLens.Data.Api.Profiles;

public class GlobalDictionariesProfile : Profile
{
    public GlobalDictionariesProfile()
    {
        CreateMap<Grpc.Service.Engine, EngineModel>();
        CreateMap<EngineEntity, Grpc.Service.Engine>();
        CreateMap<EntitiesWithPagination<EngineEntity>, Grpc.Service.GetEnginesResponse>();

        CreateMap<Grpc.Service.Market, MarketModel>();
        CreateMap<MarketEntity, Grpc.Service.Market>();
        CreateMap<EntitiesWithPagination<MarketEntity>, Grpc.Service.GetMarketsResponse>();

        CreateMap<Grpc.Service.Board, BoardModel>();
        CreateMap<BoardEntity, Grpc.Service.Board>();
        CreateMap<EntitiesWithPagination<BoardEntity>, Grpc.Service.GetBoardsResponse>();

        CreateMap<Grpc.Service.BoardGroup, BoardGroupModel>();
        CreateMap<BoardGroupEntity, Grpc.Service.BoardGroup>();
        CreateMap<EntitiesWithPagination<BoardGroupEntity>, Grpc.Service.GetBoardGroupsResponse>();

        CreateMap<Grpc.Service.Duration, DurationModel>();
        CreateMap<DurationEntity, Grpc.Service.Duration>();
        CreateMap<EntitiesWithPagination<DurationEntity>, Grpc.Service.GetDurationsResponse>();

        CreateMap<Grpc.Service.SecurityType, SecurityTypeModel>();
        CreateMap<SecurityTypeEntity, Grpc.Service.SecurityType>();
        CreateMap<EntitiesWithPagination<SecurityTypeEntity>, Grpc.Service.GetSecurityTypesResponse>();

        CreateMap<Grpc.Service.SecurityGroup, SecurityGroupModel>();
        CreateMap<SecurityGroupEntity, Grpc.Service.SecurityGroup>();
        CreateMap<EntitiesWithPagination<SecurityGroupEntity>, Grpc.Service.GetSecurityGroupsResponse>();

        CreateMap<Grpc.Service.SecurityCollection, SecurityCollectionModel>();
        CreateMap<SecurityCollectionEntity, Grpc.Service.SecurityCollection>();
        CreateMap<EntitiesWithPagination<SecurityCollectionEntity>, Grpc.Service.GetSecurityCollectionsResponse>();
    }
}