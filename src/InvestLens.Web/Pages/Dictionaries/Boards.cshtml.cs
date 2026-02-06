using InvestLens.Abstraction.Services;
using InvestLens.Data.Entities.Index;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class BoardsModel : DictionaryBasePage<Board>
{
    public BoardsModel(IBoardDictionariesGrpcClientService service, ILogger<BoardsModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "Boards";

    #endregion Protected Methods
}