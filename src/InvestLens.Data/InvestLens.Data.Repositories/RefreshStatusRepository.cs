using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities;
using InvestLens.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories;

public class RefreshStatusRepository : BaseRepository<RefreshStatus>, IRefreshStatusRepository
{
    public RefreshStatusRepository(
        InvestLensDataContext context,
        IPollyService pollyService,
        ILogger<RefreshStatusRepository> logger) : base(context, pollyService, logger)
    {
    }

    public async Task<RefreshStatus?> GetRefreshStatus(string entityName)
    {
        try
        {
            // ToDo изменить System.Net.Sockets.SocketException на более конкретный тип.
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
                refreshStatus = new RefreshStatus(entityName);
                DbSet.Add(refreshStatus);
            }
            else
            {
                refreshStatus.RefreshDate = DateTime.UtcNow;
            }

            // ToDo изменить System.Net.Sockets.SocketException на более конкретный тип.
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