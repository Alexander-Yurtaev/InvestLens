using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Index;
using InvestLens.Shared.Repositories;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories;

public class BoardRepository : BaseReadOnlyRepository<Board>, IBoardRepository
{
    public BoardRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<BoardRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<Board>> Get()
    {
        var result = await Get(1, 10);
        return result.Data;
    }

    protected override Dictionary<string, Func<Board, object>> GetSortSelectors()
    {
        return new Dictionary<string, Func<Board, object>>
        {
            {nameof(Board.BoardTitle).ToLower(), b => b.BoardTitle}
        };
    }

    protected override IQueryable<Board> GetWhereCause(IQueryable<Board> query, string filter)
    {
        if (!string.IsNullOrEmpty(filter))
        {
            filter = filter.ToUpper();
            query = query.Where(s => s.BoardTitle.ToUpper().Contains(filter));
        }
        return query;
    }
}