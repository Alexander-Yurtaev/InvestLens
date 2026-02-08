using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class EnginesModel : DictionaryBasePage<EngineModel>
{
    public EnginesModel(IEngineDictionariesGrpcClient service, ILogger<EnginesModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "Engines";

    #endregion Protected Methods
}