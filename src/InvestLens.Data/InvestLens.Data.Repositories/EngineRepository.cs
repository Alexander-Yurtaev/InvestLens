using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Index;
using InvestLens.Shared.Repositories;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories;

public class EngineRepository : BaseReadOnlyRepository<Engine>, IEngineRepository
{
    public EngineRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<EngineRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<Engine>> Get()
    {
        var result = await Get(1, 10);
        return result.Data;
    }

    protected override Dictionary<string, Func<Engine, object>> GetSortSelectors()
    {
        return new Dictionary<string, Func<Engine, object>>
        {
            {nameof(Engine.Name).ToLower(), e => e.Name},
            {nameof(Engine.Title).ToLower(), e => e.Title},
        };
    }

    protected override IQueryable<Engine> GetWhereCause(IQueryable<Engine> query, string filter)
    {
        if (!string.IsNullOrEmpty(filter))
        {
            filter = filter.ToUpper();
            query = query.Where(s => s.Name.ToUpper().Contains(filter) ||
                                     s.Title.ToUpper().Contains(filter));
        }
        return query;
    }
}