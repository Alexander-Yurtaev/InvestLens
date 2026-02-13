using InvestLens.Data.Core.Abstraction.Repositories.Dictionaries;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace InvestLens.Data.Repositories.Dictionaries;

public class SecurityCollectionRepository : BaseReadOnlyRepository<SecurityCollectionEntity>, ISecurityCollectionRepository
{
    public SecurityCollectionRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<SecurityCollectionRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<SecurityCollectionEntity>> Get()
    {
        var result = await Get(1, 10);
        return result.Entities;
    }

    protected override Dictionary<string, Expression<Func<SecurityCollectionEntity, object>>> GetSortSelectors()
    {
        return new Dictionary<string, Expression<Func<SecurityCollectionEntity, object>>>
        {
            {nameof(SecurityCollectionEntity.Name).ToLowerInvariant(), sc => sc.Name},
            {nameof(SecurityCollectionEntity.Title).ToLowerInvariant(), sc => sc.Title},
        };
    }

    protected override IQueryable<SecurityCollectionEntity> GetWhereCause(IQueryable<SecurityCollectionEntity> query, string filter)
    {
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(s => s.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
                                     s.Title.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
        }
        return query;
    }
}