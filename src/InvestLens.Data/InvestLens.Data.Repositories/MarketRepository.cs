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

    protected override Dictionary<string, Func<Market, object>> GetSortSelectors()
    {
        return new Dictionary<string, Func<Market, object>>
        {
            {nameof(Market.MarketName).ToLower(), m => m.MarketName},
            {nameof(Market.MarketTitle).ToLower(), m => m.MarketTitle},
        };
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