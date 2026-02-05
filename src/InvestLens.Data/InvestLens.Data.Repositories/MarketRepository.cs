using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Index;
using InvestLens.Shared.Repositories;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories;

public class MarketRepository : BaseReadOnlyRepository<Market>, IMarketRepository
{
    public MarketRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<MarketRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<Market>> Get()
    {
        var result = await Get(1, 10);
        return result.Data;
    }

    protected override IEnumerable<Market> GetSortAction(IEnumerable<Market> query, string sort)
    {
        if (string.IsNullOrEmpty(sort)) return query;

        sort = sort.ToLower();

        if (sort == nameof(Market.MarketName).ToLower())
        {
            return query.OrderBy(s => s.MarketName);
        }

        if (sort == nameof(Market.MarketTitle).ToLower())
        {
            return query.OrderBy(s => s.MarketTitle);
        }

        return query;
    }

    protected override IQueryable<Market> GetWhereCause(IQueryable<Market> query, string filter)
    {
        if (!string.IsNullOrEmpty(filter))
        {
            filter = filter.ToUpper();
            query = query.Where(s => s.MarketName.ToUpper().Contains(filter) ||
                                     s.MarketTitle.ToUpper().Contains(filter));
        }
        return query;
    }
}