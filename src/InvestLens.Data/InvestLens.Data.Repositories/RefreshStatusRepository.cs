using InvestLens.Data.Core.Abstraction.Repositories;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities;
using InvestLens.Shared.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories;

public class RefreshStatusRepository : BaseRepository<RefreshStatusEntity>, IRefreshStatusRepository
{
    public RefreshStatusRepository(
        InvestLensDataContext context,
        IPollyService pollyService,
        ILogger<RefreshStatusRepository> logger) : base(context, pollyService, logger)
    {
    }

    public async Task<RefreshStatusEntity?> GetRefreshStatus(string entityName)
    {
        try
        {
            var resilientPolicy = PollyService.GetResilientPolicy<System.Net.Sockets.SocketException>();
            var refreshStatus = await resilientPolicy.ExecuteAsync(async () =>
                await DbSet.AsNoTracking().FirstOrDefaultAsync(s => s.EntityName == entityName));
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
            var refreshStatus = await GetRefreshStatus(entityName);
            if (refreshStatus is null)
            {
                refreshStatus = new RefreshStatusEntity(entityName);
                DbSet.Add(refreshStatus);
            }
            else
            {
                refreshStatus.RefreshDate = DateTime.UtcNow;
            }

            var resilientPolicy = PollyService.GetResilientPolicy<System.Net.Sockets.SocketException>();
            await resilientPolicy.ExecuteAsync(async () => await Context.SaveChangesAsync());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ошибка при сохранении RefreshStatus для EntityName: {EntityName}", entityName);
            throw;
        }
    }
}