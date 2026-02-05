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

    protected override IEnumerable<Board> GetSortAction(IEnumerable<Board> query, string sort)
    {
        if (string.IsNullOrEmpty(sort)) return query;

        sort = sort.ToLower();

        if (sort == nameof(Board.BoardTitle).ToLower())
        {
            return query.OrderBy(s => s.BoardTitle);
        }

        return query;
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