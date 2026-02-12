using InvestLens.Data.Entities;

namespace InvestLens.Shared.Interfaces.Services;

public interface IDataWriterService
{
    Task<int> SaveDataAsync<TEntity>(string keyName, IEnumerable<TEntity> entities, int batchId,
        Func<Exception, Task> failBack, CancellationToken cancellationToken)
        where TEntity : BaseEntity;
}