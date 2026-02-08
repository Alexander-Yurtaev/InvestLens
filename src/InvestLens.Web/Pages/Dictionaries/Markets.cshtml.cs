using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class MarketsModel : DictionaryBasePage<MarketModel>
{
    public MarketsModel(IHttpClientFactory httpFactory, ILogger<MarketsModel> logger) : base(httpFactory, logger)
    {
    }

    #region Protected Methods

    public override string Route => "Markets";
    protected override Type ConcreteType => typeof(MarketModelWithPagination);

    #endregion Protected Methods
}