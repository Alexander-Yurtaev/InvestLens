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

    protected override IEnumerable<Duration> GetSortAction(IEnumerable<Duration> query, string sort)
    {
        if (string.IsNullOrEmpty(sort)) return query;

        sort = sort.ToLower();

        if (sort == nameof(Duration.Interval).ToLower())
        {
            return query.OrderBy(s => s.Interval);
        }

        if (sort == nameof(Duration.DurationValue).ToLower())
        {
            return query.OrderBy(s => s.DurationValue);
        }

        if (sort == nameof(Duration.Title).ToLower())
        {
            return query.OrderBy(s => s.Title);
        }

        return query;
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