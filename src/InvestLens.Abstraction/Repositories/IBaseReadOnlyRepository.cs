using InvestLens.Data.Entities;

namespace InvestLens.Abstraction.Repositories;

public interface IBaseReadOnlyRepository<TEntity> where TEntity : BaseEntity
{
    Task<List<TEntity>> Get();

    Task<IGetResult<TEntity>> Get(int page, int pageSize, string? sort, string? filter);

    Task<TEntity?> Get(int id);
}