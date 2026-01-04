using InvestLens.Abstraction.Repositories;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities;
using InvestLens.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace InvestLens.Data.Repositories
{
    public class RefreshStatusRepository : BaseRepository<RefreshStatus, Guid>, IRefreshStatusRepository
    {
        public RefreshStatusRepository(InvestLensDataContext context, ILogger<RefreshStatusRepository> logger) : base(context, logger)
        {
        }

        public async Task<RefreshStatus?> GetRefreshStatus(string entityName)
        {
            try
            {
                var refreshStatus = await DbSet.AsNoTracking().FirstOrDefaultAsync(s => s.EntityName == entityName);
                return refreshStatus;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Ошибка при получении RefreshStatus для EntityName: {EntityName}", entityName);
                throw;
            }
        }

        public async Task SetRefreshStatus(string entityName)
        {
            try
            {
                var refreshStatus = await DbSet.FirstOrDefaultAsync(s => s.EntityName == entityName);
                if (refreshStatus is null)
                {
                    refreshStatus = new RefreshStatus(entityName);
                    DbSet.Add(refreshStatus);
                }
                else
                {
                    refreshStatus.RefreshDate = DateTime.UtcNow;
                }

                await Context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Ошибка при сохранении RefreshStatus для EntityName: {EntityName}", entityName);
                throw;
            }
        }
    }
}