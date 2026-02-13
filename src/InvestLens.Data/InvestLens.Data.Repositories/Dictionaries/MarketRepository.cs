using InvestLens.Data.Core.Abstraction.Repositories.Dictionaries;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace InvestLens.Data.Repositories.Dictionaries;

public class MarketRepository : BaseReadOnlyRepository<MarketEntity>, IMarketRepository
{
    public MarketRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<MarketRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<MarketEntity>> Get()
    {
        var result = await Get(1, 10);
        return result.Entities;
    }

    protected override Dictionary<string, Expression<Func<MarketEntity, object>>> GetSortSelectors()
    {
        return new Dictionary<string, Expression<Func<MarketEntity, object>>>
        {
            {nameof(MarketEntity.MarketName).ToLowerInvariant(), m => m.MarketName},
            {nameof(MarketEntity.MarketTitle).ToLowerInvariant(), m => m.MarketTitle},
        };
    }

    protected override IQueryable<MarketEntity> GetWhereCause(IQueryable<MarketEntity> query, string filter)
    {
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(s => s.MarketName.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
                                     s.MarketTitle.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
        }
        return query;
    }
}