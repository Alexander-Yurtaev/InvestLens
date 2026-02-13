using InvestLens.Data.Core.Abstraction.Repositories.Dictionaries;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace InvestLens.Data.Repositories.Dictionaries;

public class EngineRepository : BaseReadOnlyRepository<EngineEntity>, IEngineRepository
{
    public EngineRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<EngineRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<EngineEntity>> Get()
    {
        var result = await Get(1, 10);
        return result.Entities;
    }

    protected override Dictionary<string, Expression<Func<EngineEntity, object>>> GetSortSelectors()
    {
        return new Dictionary<string, Expression<Func<EngineEntity, object>>>
        {
            {nameof(EngineEntity.Name).ToLowerInvariant(), e => e.Name},
            {nameof(EngineEntity.Title).ToLowerInvariant(), e => e.Title},
        };
    }

    protected override IQueryable<EngineEntity> GetWhereCause(IQueryable<EngineEntity> query, string filter)
    {
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(s => s.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ||
                                     s.Title.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
        }
        return query;
    }
}