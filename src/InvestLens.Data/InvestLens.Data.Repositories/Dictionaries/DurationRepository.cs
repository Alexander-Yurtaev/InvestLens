using InvestLens.Data.Core.Abstraction.Repositories.Dictionaries;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace InvestLens.Data.Repositories.Dictionaries;

public class DurationRepository : BaseReadOnlyRepository<DurationEntity>, IDurationRepository
{
    public DurationRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<DurationRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<DurationEntity>> Get()
    {
        var result = await Get(1, 10);
        return result.Entities;
    }

    protected override Dictionary<string, Expression<Func<DurationEntity, object>>> GetSortSelectors()
    {
        return new Dictionary<string, Expression<Func<DurationEntity, object>>>
        {
            {nameof(DurationEntity.Interval).ToLowerInvariant(), d => d.Interval},
            {nameof(DurationEntity.DurationValue).ToLowerInvariant(), d => d.DurationValue},
            {nameof(DurationEntity.Title).ToLowerInvariant(), d => d.Title},
        };
    }

    protected override IQueryable<DurationEntity> GetWhereCause(IQueryable<DurationEntity> query, string filter)
    {
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(s => s.Title.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
        }
        return query;
    }
}