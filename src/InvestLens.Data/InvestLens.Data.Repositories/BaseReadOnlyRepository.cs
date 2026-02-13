using InvestLens.Data.Core.Abstraction.Repositories;
using InvestLens.Data.Entities;
using InvestLens.Data.Repositories.Extensions;
using InvestLens.Shared.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using System.Linq.Expressions;

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
                var countTask = DbSet.AsNoTracking().Filter(GetWhereCause, filter).CountAsync();
                var query = DbSet.AsNoTracking().Filter(GetWhereCause, filter);
                if (!string.IsNullOrEmpty(sort))
                {
                    query = query.OrderByEx(ApplySorting, sort);
                }

                query = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                var totalItems = await countTask;
                var entities = query.ToList();
                var result = new EntitiesWithPagination<TEntity>
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                    TotalItems = totalItems,
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

    protected virtual Dictionary<string, Expression<Func<TEntity, object>>> GetSortSelectors() => new();

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

    private IOrderedQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, string sort)
    {
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

        throw new InvalidOperationException($"Unknown {sort} field.");
    }

    #endregion Private Methods
}