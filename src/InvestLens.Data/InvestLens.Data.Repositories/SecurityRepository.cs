using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities;
using InvestLens.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories;

public class SecurityRepository : BaseRepository<Security>, ISecurityRepository
{
    public SecurityRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<SecurityRepository> logger) : base(context, pollyService, logger)
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

    public override async Task<int> Add(List<Security> entities, bool orUpdate = false)
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

    public override async Task<List<Security>> Get()
    {
        var result = await Get(1, 10);
        return result.Data;
    }

    protected override IEnumerable<Security> GetSortAction(IEnumerable<Security> query, string sort)
    {
        if (string.IsNullOrEmpty(sort)) return query;
        
        sort = sort.ToLower();

        if (sort == nameof(Security.SecId).ToLower())
        {
            return query.OrderBy(s => s.SecId);
        }

        if (sort == nameof(Security.Name).ToLower())
        {
            return query.OrderBy(s => s.Name);
        }
        
        if (sort == nameof(Security.ShortName).ToLower())
        {
            return query.OrderBy(s => s.ShortName);
        }

        if (sort == nameof(Security.Type).ToLower())
        {
            return query.OrderBy(s => s.Type);
        }

        if (sort == nameof(Security.Group).ToLower())
        {
            return query.OrderBy(s => s.Group);
        }

        if (sort == nameof(Security.IsTraded).ToLower())
        {
            return query.OrderBy(s => s.IsTraded);
        }

        return query;
    }

    protected override IQueryable<Security> GetWhereCause(IQueryable<Security> query, string filter)
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