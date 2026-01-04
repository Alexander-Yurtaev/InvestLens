using InvestLens.Data.Entities;

namespace InvestLens.Abstraction.Repositories;

public interface IBaseRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
{
    Task<TEntity> Add(TEntity entity, bool orUpdate=false);

    Task<List<TEntity>> Add(List<TEntity> entities, bool orUpdate=false);

    Task<List<TEntity>> Get();

    Task<TEntity?> Get(TKey id);

    Task<TEntity> Update(TEntity entity);

    Task Delete(TKey id);
}