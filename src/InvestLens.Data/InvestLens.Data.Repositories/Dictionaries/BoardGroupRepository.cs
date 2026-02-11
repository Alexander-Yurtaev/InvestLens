using InvestLens.Data.Core.Abstraction.Repositories.Dictionaries;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories.Dictionaries;

public class BoardGroupRepository : BaseReadOnlyRepository<BoardGroupEntity>, IBoardGroupRepository
{
    public BoardGroupRepository(InvestLensDataContext context, IPollyService pollyService,
        ILogger<BoardGroupRepository> logger) : base(context, pollyService, logger)
    {
    }

    public override async Task<List<BoardGroupEntity>> Get()
    {
        var result = await Get(1, 10);
        return result.Entities;
    }

    protected override Dictionary<string, Func<BoardGroupEntity, object>> GetSortSelectors()
    {
        return new Dictionary<string, Func<BoardGroupEntity, object>>
        {
            {nameof(BoardGroupEntity.Name).ToLowerInvariant(), bg => bg.Name},
            {nameof(BoardGroupEntity.Title).ToLowerInvariant(), bg => bg.Title},
        };
    }

    protected override IQueryable<BoardGroupEntity> GetWhereCause(IQueryable<BoardGroupEntity> query, string filter)
    {
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(s => s.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
                                     s.Title.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
        }
        return query;
    }
}