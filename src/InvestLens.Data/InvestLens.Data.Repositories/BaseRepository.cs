using InvestLens.Data.Entities;
using InvestLens.Shared.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories;

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