using InvestLens.Abstraction.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvestLens.Shared.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    private readonly DbContext _context;
    private readonly ILogger<BaseRepository<TEntity>> _logger;
    private readonly DbSet<TEntity> _dbSet;

    public BaseRepository(DbContext context, ILogger<BaseRepository<TEntity>> logger)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
        _logger = logger;
    }

    public async Task<TEntity> Create(TEntity entity)
    {
        try
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании сущности");
            throw;
        }
    }

    public async Task<List<TEntity>> Get()
    {
        try
        {
            var entities = await _dbSet.ToListAsync();
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка сущностей");
            throw;
        }
    }

    public async Task<TEntity?> Get(Guid id)
    {
        try
        {
            var entity = await Find(id);
            return entity;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении сущности");
            throw;
        }
    }

    public async Task<TEntity> Update(TEntity entity)
    {
        try
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении сущности");
            throw;
        }
    }

    public async Task Delete(Guid id)
    {
        try
        {
            var entity = await Find(id, true);
            _dbSet.Remove(entity!);
            await _context.SaveChangesAsync();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении сущности");
            throw;
        }
    }

    #region Private Methods

    private async Task<TEntity?> Find(Guid id, bool throwIfNotFound = false)
    {
        var entity = await _dbSet.FindAsync(id);

        if (entity is null && throwIfNotFound)
        {
            throw new KeyNotFoundException($"Entity with Id={id} not found.");
        }

        return entity;
    }

    #endregion Private Methods
}