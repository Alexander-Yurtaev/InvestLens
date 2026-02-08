using InvestLens.Shared.Models;
using InvestLens.Shared.Models.Dictionaries;

namespace InvestLens.Shared.Interfaces.Services;

public interface IBaseDictionariesGrpcClient<TModel> where TModel : BaseModel
{
    Task<BaseModelWithPagination<TModel>?> GetEntitiesAsync(int page, int pageSize, string? sort = "", string? filter = "");
}

public interface IEngineDictionariesGrpcClient : IBaseDictionariesGrpcClient<EngineModel>
{
}

public interface IMarketDictionariesGrpcClient : IBaseDictionariesGrpcClient<MarketModel>
{
}

public interface IBoardDictionariesGrpcClient : IBaseDictionariesGrpcClient<BoardModel>
{
}

public interface IBoardGroupDictionariesGrpcClient : IBaseDictionariesGrpcClient<BoardGroupModel>
{
}

public interface IDurationDictionariesGrpcClient : IBaseDictionariesGrpcClient<DurationModel>
{
}

public interface ISecurityTypeDictionariesGrpcClient : IBaseDictionariesGrpcClient<SecurityTypeModel>
{
}

public interface ISecurityGroupDictionariesGrpcClient : IBaseDictionariesGrpcClient<SecurityGroupModel>
{
}

public interface ISecurityCollectionDictionariesGrpcClient : IBaseDictionariesGrpcClient<SecurityCollectionModel>
{
}