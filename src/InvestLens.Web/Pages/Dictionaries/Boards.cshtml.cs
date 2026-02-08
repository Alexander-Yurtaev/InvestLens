using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class BoardsModel : DictionaryBasePage<BoardModel>
{
    public BoardsModel(IHttpClientFactory httpFactory, ILogger<BoardsModel> logger) : base(httpFactory, logger)
    {
    }

    #region Protected Methods

    public override string Route => "Boards";
    protected override Type ConcreteType => typeof(BoardModelWithPagination);

    #endregion Protected Methods
}