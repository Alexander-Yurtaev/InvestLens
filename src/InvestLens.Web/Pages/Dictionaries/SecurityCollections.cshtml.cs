using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class SecurityCollectionsModel : DictionaryBasePage<SecurityCollectionModel>
{
    public SecurityCollectionsModel(ISecurityCollectionDictionariesGrpcClient service,
        ILogger<SecurityCollectionsModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "SecurityCollections";

    #endregion Protected Methods
}