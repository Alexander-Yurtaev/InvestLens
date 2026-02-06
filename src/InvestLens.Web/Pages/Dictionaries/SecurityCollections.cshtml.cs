using InvestLens.Abstraction.Services;
using InvestLens.Data.Entities.Index;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class SecurityCollectionsModel : DictionaryBasePage<SecurityCollection>
{
    public SecurityCollectionsModel(ISecurityCollectionDictionariesGrpcClientService service,
        ILogger<SecurityCollectionsModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "SecurityCollections";

    #endregion Protected Methods
}