using InvestLens.Abstraction.Services;
using InvestLens.Data.Entities.Index;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class MarketsModel : DictionaryBasePage<Market>
{
    public MarketsModel(IMarketDictionariesGrpcClientService service, ILogger<MarketsModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "Markets";

    #endregion Protected Methods
}