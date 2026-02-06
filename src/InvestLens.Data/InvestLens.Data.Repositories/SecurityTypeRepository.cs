using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Index;
using InvestLens.Shared.Repositories;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories;

public class SecurityTypeRepository : BaseReadOnlyRepository<SecurityType>, ISecurityTypeRepository
{
    public SecurityTypeRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<SecurityTypeRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<SecurityType>> Get()
    {
        var result = await Get(1, 10);
        return result.Data;
    }

    protected override Dictionary<string, Func<SecurityType, object>> GetSortSelectors()
    {
        return new Dictionary<string, Func<SecurityType, object>>
        {
            {nameof(SecurityType.SecurityTypeName).ToLower(), st => st.SecurityTypeName},
        };
    }

    protected override IQueryable<SecurityType> GetWhereCause(IQueryable<SecurityType> query, string filter)
    {
        if (!string.IsNullOrEmpty(filter))
        {
            filter = filter.ToUpper();
            query = query.Where(s => s.SecurityTypeName.ToUpper().Contains(filter));
        }
        return query;
    }
}