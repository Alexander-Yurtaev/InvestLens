using InvestLens.Abstraction.Services;
using InvestLens.Data.Entities.Index;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class BoardGroupsModel : DictionaryBasePage<BoardGroup>
{
    public BoardGroupsModel(IBoardGroupDictionariesGrpcClientService service, ILogger<BoardGroupsModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "BoardGroups";

    #endregion Protected Methods
}