using InvestLens.Data.Entities;

namespace InvestLens.Data.Core.Abstraction.Repositories;

public interface IRefreshStatusRepository : IBaseRepository<RefreshStatusEntity>
{
    Task<RefreshStatusEntity?> GetRefreshStatus(string entityName);

    Task SetRefreshStatus(string entityName);
}