using InvestLens.Data.Entities;

namespace InvestLens.Abstraction.Repositories;

public interface IBaseRepository<TEntity, TKey> where TEntity : BaseEntity<TKey> where TKey : struct
{
    Task<TEntity> Add(TEntity entity, bool orUpdate = false);

    Task<int> Add(List<TEntity> entities, bool orUpdate = false);

    Task<List<TEntity>> Get();

    Task<IGetResult<TEntity, TKey>> Get(int page, int pageSize, string? sort, string? filter);

    Task<TEntity?> Get(TKey id);

    Task<TEntity> Update(TEntity entity);

    Task Delete(TKey id);
}