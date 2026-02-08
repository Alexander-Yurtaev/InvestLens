using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class MarketsModel : DictionaryBasePage<MarketModel>
{
    public MarketsModel(IMarketDictionariesGrpcClient service, ILogger<MarketsModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "Markets";

    #endregion Protected Methods
}