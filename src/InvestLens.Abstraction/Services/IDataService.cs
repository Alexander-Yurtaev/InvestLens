using InvestLens.Data.Entities;

namespace InvestLens.Abstraction.Services;

public interface IDataService
{
    Task<int> SaveDataAsync<TEntity>(string keyName, IEnumerable<TEntity> entities, int batchId,
        Func<Exception, Task> failBack)
        where TEntity : BaseEntity;
}