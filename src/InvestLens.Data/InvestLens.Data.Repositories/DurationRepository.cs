using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Index;
using InvestLens.Shared.Repositories;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories;

public class DurationRepository : BaseReadOnlyRepository<Duration>, IDurationRepository
{
    public DurationRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<DurationRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<Duration>> Get()
    {
        var result = await Get(1, 10);
        return result.Data;
    }

    protected override Dictionary<string, Func<Duration, object>> GetSortSelectors()
    {
        return new Dictionary<string, Func<Duration, object>>
        {
            {nameof(Duration.Interval).ToLower(), d => d.Interval},
            {nameof(Duration.DurationValue).ToLower(), d => d.DurationValue},
            {nameof(Duration.Title).ToLower(), d => d.Title},
        };
    }

    protected override IQueryable<Duration> GetWhereCause(IQueryable<Duration> query, string filter)
    {
        if (!string.IsNullOrEmpty(filter))
        {
            filter = filter.ToUpper();
            query = query.Where(s => s.Title.ToUpper().Contains(filter));
        }
        return query;
    }
}