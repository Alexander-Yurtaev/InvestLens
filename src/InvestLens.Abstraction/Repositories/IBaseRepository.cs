using InvestLens.Data.Entities;

namespace InvestLens.Abstraction.Repositories;

public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity> Add(TEntity entity, bool orUpdate = false);

    Task<int> Add(List<TEntity> entities, bool orUpdate = false);

    Task<List<TEntity>> Get();

    Task<IGetResult<TEntity>> Get(int page, int pageSize, string? sort, string? filter);

    Task<TEntity?> Get(Guid id);

    Task<TEntity> Update(TEntity entity);

    Task Delete(Guid id);
}