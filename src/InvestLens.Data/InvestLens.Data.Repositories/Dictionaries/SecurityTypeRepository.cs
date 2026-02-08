using InvestLens.Data.Core.Abstraction.Repositories.Dictionaries;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories.Dictionaries;

public class SecurityTypeRepository : BaseReadOnlyRepository<SecurityTypeEntity>, ISecurityTypeRepository
{
    public SecurityTypeRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<SecurityTypeRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<SecurityTypeEntity>> Get()
    {
        var result = await Get(1, 10);
        return result.Entities;
    }

    public async Task<IEnumerable<SecurityTypeEntity>> GetAll()
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

    protected override Dictionary<string, Func<SecurityTypeEntity, object>> GetSortSelectors()
    {
        return new Dictionary<string, Func<SecurityTypeEntity, object>>
        {
            {nameof(SecurityTypeEntity.SecurityTypeName).ToLowerInvariant(), st => st.SecurityTypeName},
            {nameof(SecurityTypeEntity.SecurityTypeTitle).ToLowerInvariant(), st => st.SecurityTypeTitle},
        };
    }

    protected override IQueryable<SecurityTypeEntity> GetWhereCause(IQueryable<SecurityTypeEntity> query, string filter)
    {
        if (!string.IsNullOrEmpty(filter))
        {
            filter = filter.ToUpper();
            query = query.Where(s => s.SecurityTypeName.ToUpper().Contains(filter) ||
                                     s.SecurityTypeTitle.ToUpper().Contains(filter));
        }
        return query;
    }
}