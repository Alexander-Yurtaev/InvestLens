using InvestLens.Data.Entities;

namespace InvestLens.Data.Core.Abstraction.Repositories;

public interface IBaseRepository<TEntity> : IBaseReadOnlyRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity> Add(TEntity entity, bool orUpdate = false);

    Task<int> Add(List<TEntity> entities, bool orUpdate = false);

    Task<TEntity> Update(TEntity entity);

    Task Delete(int id);
}