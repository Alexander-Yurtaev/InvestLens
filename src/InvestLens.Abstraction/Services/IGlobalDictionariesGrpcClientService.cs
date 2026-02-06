using InvestLens.Abstraction.DTOs;
using InvestLens.Data.Entities;
using InvestLens.Data.Entities.Index;

namespace InvestLens.Abstraction.Services;

public interface IBaseDictionariesGrpcClientService<TEntity> where TEntity : BaseEntity
{
    Task<BaseEntityDto<TEntity>?> GetEntitiesAsync(int page, int pageSize, string? sort = "", string? filter = "");
}

public interface IEngineDictionariesGrpcClientService : IBaseDictionariesGrpcClientService<Engine>
{
}

public interface IMarketDictionariesGrpcClientService : IBaseDictionariesGrpcClientService<Market>
{
}

public interface IBoardDictionariesGrpcClientService : IBaseDictionariesGrpcClientService<Board>
{
}

public interface IBoardGroupDictionariesGrpcClientService : IBaseDictionariesGrpcClientService<BoardGroup>
{
}

public interface IDurationDictionariesGrpcClientService : IBaseDictionariesGrpcClientService<Duration>
{
}

public interface ISecurityTypeDictionariesGrpcClientService : IBaseDictionariesGrpcClientService<SecurityType>
{
}

public interface ISecurityGroupDictionariesGrpcClientService : IBaseDictionariesGrpcClientService<SecurityGroup>
{
}

public interface ISecurityCollectionDictionariesGrpcClientService : IBaseDictionariesGrpcClientService<SecurityCollection>
{
}