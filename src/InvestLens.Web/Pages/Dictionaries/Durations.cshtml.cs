using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class DurationsModel : DictionaryBasePage<DurationModel>
{
    public DurationsModel(IDurationDictionariesGrpcClient service, ILogger<DurationsModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "Durations";

    #endregion Protected Methods
}