using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class EnginesModel : DictionaryBasePage<EngineModel>
{
    public EnginesModel(IHttpClientFactory httpFactory, ILogger<EnginesModel> logger) : base(httpFactory, logger)
    {
    }

    #region Protected Methods

    public override string Route => "Engines";
    protected override Type ConcreteType => typeof(EngineModelWithPagination);

    #endregion Protected Methods
}