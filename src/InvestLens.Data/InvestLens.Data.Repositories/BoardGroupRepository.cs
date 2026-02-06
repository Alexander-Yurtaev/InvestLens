using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Index;
using InvestLens.Shared.Repositories;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories;

public class BoardGroupRepository : BaseReadOnlyRepository<BoardGroup>, IBoardGroupRepository
{
    public BoardGroupRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<BoardGroupRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<BoardGroup>> Get()
    {
        var result = await Get(1, 10);
        return result.Data;
    }

    protected override Dictionary<string, Func<BoardGroup, object>> GetSortSelectors()
    {
        return new Dictionary<string, Func<BoardGroup, object>>
        {
            {nameof(BoardGroup.Name).ToLower(), bg => bg.Name},
            {nameof(BoardGroup.Title).ToLower(), bg => bg.Title},
        };
    }

    protected override IQueryable<BoardGroup> GetWhereCause(IQueryable<BoardGroup> query, string filter)
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