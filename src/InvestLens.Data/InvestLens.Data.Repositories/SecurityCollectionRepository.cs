using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Index;
using InvestLens.Shared.Repositories;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories;

public class SecurityCollectionRepository : BaseReadOnlyRepository<SecurityCollection>, ISecurityCollectionRepository
{
    public SecurityCollectionRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<SecurityCollectionRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<SecurityCollection>> Get()
    {
        var result = await Get(1, 10);
        return result.Data;
    }

    protected override IEnumerable<SecurityCollection> GetSortAction(IEnumerable<SecurityCollection> query, string sort)
    {
        if (string.IsNullOrEmpty(sort)) return query;

        sort = sort.ToLower();

        if (sort == nameof(SecurityGroup.Name).ToLower())
        {
            return query.OrderBy(s => s.Name);
        }

        if (sort == nameof(SecurityGroup.Title).ToLower())
        {
            return query.OrderBy(s => s.Title);
        }

        return query;
    }

    protected override IQueryable<SecurityCollection> GetWhereCause(IQueryable<SecurityCollection> query, string filter)
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