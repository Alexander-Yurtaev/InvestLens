using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Entities;
using InvestLens.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly.Wrap;

namespace InvestLens.Shared.Repositories;

public abstract class BaseRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey> where TEntity : BaseEntity<TKey> where TKey : struct
{
    protected readonly DbContext Context;
    protected readonly IPollyService PollyService;
    protected AsyncPolicyWrap ResilientPolicy;
    protected readonly ILogger<BaseRepository<TEntity, TKey>> Logger;
    protected readonly DbSet<TEntity> DbSet;

    protected BaseRepository(DbContext context, IPollyService pollyService, ILogger<BaseRepository<TEntity, TKey>> logger)
    {
        Context = context;
        PollyService = pollyService;
        // ToDo изменить System.Net.Sockets.SocketException на более конкретный тип.
        ResilientPolicy = PollyService.GetResilientPolicy<System.Net.Sockets.SocketException>();

        DbSet = context.Set<TEntity>();
        Logger = logger;
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
            Logger.LogError(ex, "Ошибка при создании сущности Id: {Id}", entity.Id);
            throw;
        }
    }

    public virtual async Task<int> Add(List<TEntity> entities, bool orUpdate=false)
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
            Logger.LogError(ex, "Ошибка при создании сущности");
            throw;
        }
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
            Logger.LogError(ex, "Ошибка при получении списка сущностей");
            throw;
        }
    }

    public virtual async Task<IGetResult<TEntity, TKey>> Get(int page, int pageSize, string? sort = "", string? filter = "")
    {
        try
        {
            var entities = await ResilientPolicy.ExecuteAsync(async () =>
            {
                var items = await DbSet.AsNoTracking().Filter<TEntity, TKey>(GetWhereCause, filter).ToListAsync();
                var query = items
                    .OrderByEx<TEntity, TKey>(GetSortAction, sort)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                var data = query.ToList();
                var result = new GetResult<TEntity, TKey>
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)items.Count / pageSize),
                    TotalItems = items.Count,
                    Data = data
                };

                return result;
            });
            return entities;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ошибка при получении списка сущностей");
            throw;
        }
    }

    protected virtual IEnumerable<TEntity> GetSortAction(IEnumerable<TEntity> query, string sort)
    {
        return query;
    }

    protected virtual IQueryable<TEntity> GetWhereCause(IQueryable<TEntity> query, string filter)
    {
        return query;
    }

    public virtual async Task<TEntity?> Get(TKey id)
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
            Logger.LogError(ex, "Ошибка при получении сущности Id: {Id}", id);
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
            Logger.LogError(ex, "Ошибка при обновлении сущности Id: {Id}", entity.Id);
            throw;
        }
    }

    public virtual async Task Delete(TKey id)
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
            Logger.LogError(ex, "Ошибка при удалении сущности Id: {Id}", id);
            throw;
        }
    }

    #region Private Methods

    private async Task<TEntity?> Find(TKey id, bool throwIfNotFound = false)
    {
        var entity = await ResilientPolicy.ExecuteAsync(async () => await DbSet.FindAsync(id));
        if (entity is not null || !throwIfNotFound) return entity;
        
        Logger.LogWarning("Сущность не найдена с Id: {Id}", id);
        throw new KeyNotFoundException($"Entity not found.");
    }

    #endregion Private Methods
}