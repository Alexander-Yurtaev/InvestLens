using InvestLens.Data.Core.Abstraction.Repositories;
using InvestLens.Data.Entities;
using InvestLens.Data.Repositories.Extensions;
using InvestLens.Shared.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;

namespace InvestLens.Data.Repositories;

public abstract class BaseReadOnlyRepository<TEntity> : IBaseReadOnlyRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly DbContext Context;
    protected readonly IPollyService PollyService;
    protected AsyncPolicy ResilientPolicy;
    protected readonly ILogger<BaseReadOnlyRepository<TEntity>> Logger;
    protected readonly DbSet<TEntity> DbSet;

    protected BaseReadOnlyRepository(DbContext context, IPollyService pollyService, ILogger<BaseReadOnlyRepository<TEntity>> logger)
    {
        Context = context;
        PollyService = pollyService;
        ResilientPolicy = PollyService.GetResilientPolicy<System.Net.Sockets.SocketException>();

        DbSet = context.Set<TEntity>();
        Logger = logger;
    }

    public virtual async Task<List<TEntity>> Get()
    {
        try
        {
            var entities = await ResilientPolicy.ExecuteAsync(async () => await DbSet.AsNoTracking().ToListAsync());
            return entities;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while getting entity list");
            throw;
        }
    }

    public virtual async Task<EntitiesWithPagination<TEntity>> Get(int page, int pageSize, string? sort = "", string? filter = "")
    {
        try
        {
            var entities = await ResilientPolicy.ExecuteAsync(async () =>
            {
                var items = await DbSet.AsNoTracking().Filter(GetWhereCause, filter).ToListAsync();
                var query = items
                    .OrderByEx(ApplySorting, sort)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                var entities = query.ToList();
                var result = new EntitiesWithPagination<TEntity>
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)items.Count / pageSize),
                    TotalItems = items.Count,
                    Entities = entities
                };

                return result;
            });

            return entities;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while getting entity list");
            throw;
        }
    }

    protected virtual Dictionary<string, Func<TEntity, object>> GetSortSelectors() => new();

    protected virtual IQueryable<TEntity> GetWhereCause(IQueryable<TEntity> query, string filter)
    {
        return query;
    }

    public virtual async Task<TEntity?> Get(int id)
    {
        try
        {
            var entity = await Find(id);
            return entity;
        }
        catch (KeyNotFoundException ex)
        {
            Logger.LogError(ex, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while getting entity with Id: {Id}", id);
            throw;
        }
    }

    #region Protected Methods

    protected async Task<TEntity?> Find(int id, bool throwIfNotFound = false)
    {
        var entity = await ResilientPolicy.ExecuteAsync(async () => await DbSet.FindAsync(id));
        if (entity is not null || !throwIfNotFound) return entity;

        Logger.LogWarning("Entity not found with Id: {Id}", id);
        throw new KeyNotFoundException($"Entity not found.");
    }

    #endregion Protected Methods

    #region Private Methods

    private IEnumerable<TEntity> ApplySorting(IEnumerable<TEntity> query, string sort)
    {
        if (string.IsNullOrEmpty(sort)) return query;

        var isDesc = false;
        sort = sort.ToLowerInvariant();
        if (sort.EndsWith("_desc", StringComparison.OrdinalIgnoreCase))
        {
            isDesc = true;
            sort = sort.Substring(0, sort.Length - "desc".Length - 1);
        }

        foreach (var action in GetSortSelectors())
        {
            if (sort == action.Key.ToLowerInvariant())
            {
                return isDesc ? query.OrderByDescending(action.Value) : query.OrderBy(action.Value);
            }
        }

        return query;
    }

    #endregion Private Methods
}

public abstract class BaseRepository<TEntity> : BaseReadOnlyRepository<TEntity> where TEntity : BaseEntity
{
    protected BaseRepository(DbContext context, IPollyService pollyService, ILogger<BaseRepository<TEntity>> logger) :
        base(context, pollyService, logger)
    {
    }

    public virtual async Task<TEntity> Add(TEntity entity, bool orUpdate = false)
    {
        try
        {
            if (orUpdate)
            {
                var savedEntity = await Get(entity.Id);
                if (savedEntity is not null)
                {
                    await ResilientPolicy.ExecuteAsync(async () => await Task.FromResult(DbSet.Update(entity)));
                }
                else
                {
                    await ResilientPolicy.ExecuteAsync(async () => await Task.FromResult(DbSet.Add(entity)));
                }
            }
            else
            {
                await ResilientPolicy.ExecuteAsync(async () => await Task.FromResult(DbSet.Add(entity)));
            }

            await ResilientPolicy.ExecuteAsync(async () => await Context.SaveChangesAsync());
            return entity;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while creating entity with Id: {Id}", entity.Id);
            throw;
        }
    }

    public virtual async Task<int> Add(List<TEntity> entities, bool orUpdate = false)
    {
        try
        {
            if (orUpdate)
            {
                foreach (var entity in entities)
                {
                    var savedEntity = await Get(entity.Id);
                    if (savedEntity is not null)
                    {
                        await ResilientPolicy.ExecuteAsync(async () => await Task.FromResult(DbSet.Update(entity)));
                    }
                    else
                    {
                        await ResilientPolicy.ExecuteAsync(async () => await Task.FromResult(DbSet.Add(entity)));
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
            Logger.LogError(ex, "Error while creating entity");
            throw;
        }
    }

    public virtual async Task<TEntity> Update(TEntity entity)
    {
        try
        {
            await ResilientPolicy.ExecuteAsync(async () =>
            {
                DbSet.Update(entity);
                await Task.CompletedTask;
            });
            await ResilientPolicy.ExecuteAsync(async () => await Context.SaveChangesAsync());
            return entity;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while updating entity with Id: {Id}", entity.Id);
            throw;
        }
    }

    public virtual async Task Delete(int id)
    {
        try
        {
            var entity = await Find(id, true);
            await ResilientPolicy.ExecuteAsync(async () =>
            {
                DbSet.Remove(entity!);
                await Task.CompletedTask;
            });
            await ResilientPolicy.ExecuteAsync(async () => await Context.SaveChangesAsync());
        }
        catch (KeyNotFoundException ex)
        {
            Logger.LogError(ex, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while deleting entity with Id: {Id}", id);
            throw;
        }
    }
}