using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities;
using InvestLens.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories;

public class SecurityRepository : BaseRepository<Security, Guid>, ISecurityRepository
{
    public SecurityRepository(InvestLensDataContext context, IPollyService pollyService, ILogger<SecurityRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<Security> Add(Security entity, bool orUpdate = false)
    {
        try
        {
            if (orUpdate)
            {
                var entityFromDb = await ResilientPolicy.ExecuteAsync(async () =>
                    await DbSet.AsNoTracking().FirstOrDefaultAsync(s => s.SecId == entity.SecId));

                var id = entityFromDb?.Id;
                if (id.HasValue)
                {
                    entity.Id = id.Value;
                    await ResilientPolicy.ExecuteAsync(async () =>
                    {
                        DbSet.Update(entity);
                        await Task.CompletedTask;
                    });
                }
                else
                {
                    await ResilientPolicy.ExecuteAsync(async () =>
                    {
                        DbSet.Add(entity);
                        await Task.CompletedTask;
                    });
                }
            }
            else
            {
                await ResilientPolicy.ExecuteAsync(async () =>
                {
                    DbSet.Add(entity);
                    await Task.CompletedTask;
                });
            }

            await ResilientPolicy.ExecuteAsync(async () => await Context.SaveChangesAsync());
            return entity;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ошибка при создании/обновлении сущности SecId: {SecId}", entity.SecId);
            throw;
        }
    }

    public override async Task<List<Security>> Add(List<Security> entities, bool orUpdate = false)
    {
        try
        {
            if (orUpdate)
            {
                foreach (var entity in entities)
                {
                    var entityFromDb = await ResilientPolicy.ExecuteAsync(async () =>
                        await DbSet.AsNoTracking().FirstOrDefaultAsync(s => s.SecId == entity.SecId));

                    var id = entityFromDb?.Id;
                    if (id.HasValue)
                    {
                        entity.Id = id.Value;
                        await ResilientPolicy.ExecuteAsync(async () =>
                        {
                            DbSet.Update(entity);
                            await Task.CompletedTask;
                        });
                    }
                    else
                    {
                        await ResilientPolicy.ExecuteAsync(async () =>
                        {
                            DbSet.Add(entity);
                            await Task.CompletedTask;
                        });
                    }
                }
            }
            else
            {
                await ResilientPolicy.ExecuteAsync(async () =>
                {
                    DbSet.AddRange(entities);
                    await Task.CompletedTask;
                });
            }

            await ResilientPolicy.ExecuteAsync(async () => await Context.SaveChangesAsync());
            return entities;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ошибка при создании/обновлении сущности");
            throw;
        }
    }
}