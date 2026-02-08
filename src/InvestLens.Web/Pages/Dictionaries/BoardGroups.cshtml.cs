using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class BoardGroupsModel : DictionaryBasePage<BoardGroupModel>
{
    public BoardGroupsModel(IHttpClientFactory httpFactory, ILogger<BoardGroupsModel> logger) : base(httpFactory, logger)
    {
    }

    #region Protected Methods

    public override string Route => "BoardGroups";
    protected override Type ConcreteType => typeof(BoardGroupModelWithPagination);

    #endregion Protected Methods
}