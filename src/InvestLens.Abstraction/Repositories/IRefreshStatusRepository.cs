using InvestLens.Data.Entities;

namespace InvestLens.Abstraction.Repositories;

public interface IRefreshStatusRepository : IBaseRepository<RefreshStatus>
{
    Task<RefreshStatus?> GetRefreshStatus(string entityName);

    Task SetRefreshStatus(string entityName);
}