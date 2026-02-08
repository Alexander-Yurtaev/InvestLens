using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class DurationsModel : DictionaryBasePage<DurationModel>
{
    public DurationsModel(IHttpClientFactory httpFactory, ILogger<DurationsModel> logger) : base(httpFactory, logger)
    {
    }

    #region Protected Methods

    public override string Route => "Durations";
    protected override Type ConcreteType => typeof(DurationModelWithPagination);

    #endregion Protected Methods
}