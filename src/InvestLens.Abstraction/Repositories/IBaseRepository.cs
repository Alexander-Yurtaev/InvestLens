namespace InvestLens.Abstraction.Repositories;

public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<TEntity> Create(TEntity entity);

    Task<List<TEntity>> Get();

    Task<TEntity?> Get(Guid id);

    Task<TEntity> Update(TEntity entity);

    Task Delete(Guid id);
}