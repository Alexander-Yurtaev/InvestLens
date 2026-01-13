using InvestLens.Abstraction.Repositories;
using InvestLens.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvestLens.Shared.Repositories;

public abstract class BaseRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey> where TEntity : BaseEntity<TKey> where TKey : struct
{
    protected readonly DbContext Context;
    protected readonly ILogger<BaseRepository<TEntity, TKey>> Logger;
    protected readonly DbSet<TEntity> DbSet;

    protected BaseRepository(DbContext context, ILogger<BaseRepository<TEntity, TKey>> logger)
    {
        Context = context;
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
                    DbSet.Update(entity);
                }
                else
                {
                    DbSet.Add(entity);
                }
            }
            else
            {
                DbSet.Add(entity);
            }
                
            await Context.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ошибка при создании сущности Id: {Id}", entity.Id);
            throw;
        }
    }

    public virtual async Task<List<TEntity>> Add(List<TEntity> entities, bool orUpdate=false)
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
                        DbSet.Update(entity);
                    }
                    else
                    {
                        DbSet.Add(entity);
                    }
                }
            }
            else
            {
                DbSet.AddRange(entities);
            }

            await Context.SaveChangesAsync();
            return entities;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ошибка при создании сущности");
            throw;
        }
    }

    public async Task<List<TEntity>> Get()
    {
        try
        {
            var entities = await DbSet.AsNoTracking().ToListAsync();
            return entities;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ошибка при получении списка сущностей");
            throw;
        }
    }

    public async Task<TEntity?> Get(TKey id)
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

    public async Task<TEntity> Update(TEntity entity)
    {
        try
        {
            DbSet.Update(entity);
            await Context.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ошибка при обновлении сущности Id: {Id}", entity.Id);
            throw;
        }
    }

    public async Task Delete(TKey id)
    {
        try
        {
            var entity = await Find(id, true);
            DbSet.Remove(entity!);
            await Context.SaveChangesAsync();
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
        var entity = await DbSet.FindAsync(id);

        if (entity is null && throwIfNotFound)
        {
            Logger.LogWarning("Сущность не найдена с Id: {Id}", id);
            throw new KeyNotFoundException($"Entity not found.");
        }

        return entity;
    }

    #endregion Private Methods
}