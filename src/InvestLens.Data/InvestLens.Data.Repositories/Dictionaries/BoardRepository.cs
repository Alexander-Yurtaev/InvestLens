using InvestLens.Data.Core.Abstraction.Repositories.Dictionaries;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories.Dictionaries;

public class BoardRepository : BaseReadOnlyRepository<BoardEntity>, IBoardRepository
{
    public BoardRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<BoardRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<BoardEntity>> Get()
    {
        var result = await Get(1, 10);
        return result.Entities;
    }

    protected override Dictionary<string, Func<BoardEntity, object>> GetSortSelectors()
    {
        return new Dictionary<string, Func<BoardEntity, object>>
        {
            {nameof(BoardEntity.BoardTitle).ToLowerInvariant(), b => b.BoardTitle}
        };
    }

    protected override IQueryable<BoardEntity> GetWhereCause(IQueryable<BoardEntity> query, string filter)
    {
        if (!string.IsNullOrEmpty(filter))
        {
            filter = filter.ToUpper();
            query = query.Where(s => s.BoardTitle.ToUpper().Contains(filter));
        }
        return query;
    }
}