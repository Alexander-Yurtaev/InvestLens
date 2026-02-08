using InvestLens.Data.Entities;

namespace InvestLens.Data.Core.Abstraction.Repositories;

public interface IBaseReadOnlyRepository<TEntity> where TEntity : BaseEntity
{
    Task<List<TEntity>> Get();

    Task<EntitiesWithPagination<TEntity>> Get(int page, int pageSize, string? sort, string? filter);

    Task<TEntity?> Get(int id);
}