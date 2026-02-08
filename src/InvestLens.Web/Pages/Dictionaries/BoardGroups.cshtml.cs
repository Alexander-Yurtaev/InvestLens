using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class BoardGroupsModel : DictionaryBasePage<BoardGroupModel>
{
    public BoardGroupsModel(IBoardGroupDictionariesGrpcClient service, ILogger<BoardGroupsModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "BoardGroups";

    #endregion Protected Methods
}