using InvestLens.Abstraction.Services;
using InvestLens.Data.Entities.Index;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class DurationsModel : DictionaryBasePage<Duration>
{
    public DurationsModel(IDurationDictionariesGrpcClientService service, ILogger<DurationsModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "Durations";

    #endregion Protected Methods
}