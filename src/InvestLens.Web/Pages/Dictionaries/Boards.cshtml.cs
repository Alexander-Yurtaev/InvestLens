using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class BoardsModel : DictionaryBasePage<BoardModel>
{
    public BoardsModel(IBoardDictionariesGrpcClient service, ILogger<BoardsModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "Boards";

    #endregion Protected Methods
}