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

    protected override IEnumerable<Engine> GetSortAction(IEnumerable<Engine> query, string sort)
    {
        if (string.IsNullOrEmpty(sort)) return query;

        sort = sort.ToLower();

        if (sort == nameof(Engine.Name).ToLower())
        {
            return query.OrderBy(s => s.Name);
        }

        if (sort == nameof(Engine.Title).ToLower())
        {
            return query.OrderBy(s => s.Title);
        }

        return query;
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