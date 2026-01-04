using InvestLens.Abstraction.Repositories;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities;
using InvestLens.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories;

public class SecurityRepository : BaseRepository<Security, Guid>, ISecurityRepository
{
    public SecurityRepository(InvestLensDataContext context, ILogger<SecurityRepository> logger) : base(context, logger)
    {
    }

    public override async Task<Security> Add(Security entity, bool orUpdate = false)
    {
        try
        {
            if (orUpdate)
            {
                var id = (await DbSet.AsNoTracking().FirstOrDefaultAsync(s => s.SecId == entity.SecId))?.Id;
                if (id.HasValue)
                {
                    entity.Id = id.Value;
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
                    var id = (await DbSet.AsNoTracking().FirstOrDefaultAsync(s => s.SecId == entity.SecId))?.Id;
                    if (id.HasValue)
                    {
                        entity.Id = id.Value;
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
            Logger.LogError(ex, "Ошибка при создании/обновлении сущности");
            throw;
        }
    }
}