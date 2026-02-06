using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Index;
using InvestLens.Shared.Repositories;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories;

public class SecurityGroupRepository : BaseReadOnlyRepository<SecurityGroup>, ISecurityGroupRepository
{
    public SecurityGroupRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<SecurityGroupRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<SecurityGroup>> Get()
    {
        var result = await Get(1, 10);
        return result.Data;
    }

    protected override Dictionary<string, Func<SecurityGroup, object>> GetSortSelectors()
    {
        return new Dictionary<string, Func<SecurityGroup, object>>
        {
            {nameof(SecurityGroup.Name).ToLower(), sg => sg.Name},
            {nameof(SecurityGroup.Title).ToLower(), sg => sg.Title},
        };
    }

    protected override IQueryable<SecurityGroup> GetWhereCause(IQueryable<SecurityGroup> query, string filter)
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