using InvestLens.Data.Core.Abstraction.Repositories.Dictionaries;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories.Dictionaries;

public class SecurityGroupRepository : BaseReadOnlyRepository<SecurityGroupEntity>, ISecurityGroupRepository
{
    public SecurityGroupRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<SecurityGroupRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<SecurityGroupEntity>> Get()
    {
        var result = await Get(1, 10);
        return result.Entities;
    }

    public async Task<IEnumerable<SecurityGroupEntity>> GetAll()
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

    protected override Dictionary<string, Func<SecurityGroupEntity, object>> GetSortSelectors()
    {
        return new Dictionary<string, Func<SecurityGroupEntity, object>>
        {
            {nameof(SecurityGroupEntity.Name).ToLowerInvariant(), sg => sg.Name},
            {nameof(SecurityGroupEntity.Title).ToLowerInvariant(), sg => sg.Title},
            {nameof(SecurityGroupEntity.IsHidden).ToLowerInvariant(), sg => sg.IsHidden},
        };
    }

    protected override IQueryable<SecurityGroupEntity> GetWhereCause(IQueryable<SecurityGroupEntity> query, string filter)
    {
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(s => s.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
                                     s.Title.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
        }
        return query;
    }
}