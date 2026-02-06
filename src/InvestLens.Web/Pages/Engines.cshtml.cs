using InvestLens.Abstraction.Services;
using InvestLens.Data.Entities.Index;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages;

public class EnginesModel : DictionaryBasePage<Engine>
{
    public EnginesModel(IEngineDictionariesGrpcClientService service, ILogger<EnginesModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "Engines";

    #endregion Protected Methods
}