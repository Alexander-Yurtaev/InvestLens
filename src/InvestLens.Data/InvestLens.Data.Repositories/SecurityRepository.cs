using InvestLens.Data.Core.Abstraction.Repositories;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities;
using InvestLens.Shared.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories;

public class SecurityRepository : BaseRepository<SecurityEntity>, ISecurityRepository
{
    public SecurityRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<SecurityRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<SecurityEntity> Add(SecurityEntity entity, bool orUpdate = false)
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

    public override async Task<int> Add(List<SecurityEntity> entities, bool orUpdate = false)
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

            var affected = await ResilientPolicy.ExecuteAsync(async () => await Context.SaveChangesAsync());
            return affected;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ошибка при создании/обновлении сущности");
            throw;
        }
    }

    public override async Task<List<SecurityEntity>> Get()
    {
        var result = await Get(1, 10);
        return result.Entities;
    }

    protected override Dictionary<string, Func<SecurityEntity, object>> GetSortSelectors()
    {
        return new Dictionary<string, Func<SecurityEntity, object>>
        {
            {nameof(SecurityEntity.SecId).ToLowerInvariant(), s => s.SecId},
            {nameof(SecurityEntity.Name).ToLowerInvariant(), s => s.Name},
            {nameof(SecurityEntity.ShortName).ToLowerInvariant(), s => s.ShortName},
            {nameof(SecurityEntity.Type).ToLowerInvariant(), s => s.Type},
            {nameof(SecurityEntity.Group).ToLowerInvariant(), s => s.Group},
            {nameof(SecurityEntity.IsTraded).ToLowerInvariant(), s => s.IsTraded},
        };
    }

    protected override IQueryable<SecurityEntity> GetWhereCause(IQueryable<SecurityEntity> query, string filter)
    {
        if (!string.IsNullOrEmpty(filter))
        {
            filter = filter.ToUpper();
            query = query.Where(s => s.SecId.Contains(filter) ||
                                     s.Name.ToUpper().Contains(filter) ||
                                     s.ShortName.ToUpper().Contains(filter));
        }
        return query;
    }
}