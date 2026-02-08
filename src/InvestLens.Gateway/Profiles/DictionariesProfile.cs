using AutoMapper;
using InvestLens.Shared.Models.Dictionaries;

namespace InvestLens.Gateway.Profiles;

public class DictionariesProfile : Profile
{
    public DictionariesProfile()
    {
        CreateMap<EngineModel, Grpc.Service.Engine>().ReverseMap();
        CreateMap<Grpc.Service.GetEnginesResponse, EngineModelWithPagination>()
            .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.Models, opt => opt.MapFrom(src => src.Data))
            ;

        CreateMap<BoardModel, Grpc.Service.Board>().ReverseMap();
        CreateMap<Grpc.Service.GetBoardsResponse, BoardModelWithPagination>()
            .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.Models, opt => opt.MapFrom(src => src.Data))
            ;

        CreateMap<BoardGroupModel, Grpc.Service.BoardGroup>().ReverseMap();
        CreateMap<Grpc.Service.GetBoardGroupsResponse, BoardGroupModelWithPagination>()
            .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.Models, opt => opt.MapFrom(src => src.Data))
            ;

        CreateMap<MarketModel, Grpc.Service.Market>().ReverseMap();
        CreateMap<Grpc.Service.GetMarketsResponse, MarketModelWithPagination>()
            .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.Models, opt => opt.MapFrom(src => src.Data))
            ;

        CreateMap<DurationModel, Grpc.Service.Duration>().ReverseMap();
        CreateMap<Grpc.Service.GetDurationsResponse, DurationModelWithPagination>()
            .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.Models, opt => opt.MapFrom(src => src.Data))
            ;

        CreateMap<SecurityTypeModel, Grpc.Service.SecurityType>().ReverseMap();
        CreateMap<Grpc.Service.GetSecurityTypesResponse, SecurityTypeModelWithPagination>()
            .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.Models, opt => opt.MapFrom(src => src.Data))
            ;

        CreateMap<SecurityGroupModel, Grpc.Service.SecurityGroup>().ReverseMap();
        CreateMap<Grpc.Service.GetSecurityGroupsResponse, SecurityGroupModelWithPagination>()
            .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.Models, opt => opt.MapFrom(src => src.Data))
            ;

        CreateMap<SecurityCollectionModel, Grpc.Service.SecurityCollection>().ReverseMap();
        CreateMap<Grpc.Service.GetSecurityCollectionsResponse, SecurityCollectionModelWithPagination>()
            .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.Models, opt => opt.MapFrom(src => src.Data))
            ;
    }
}