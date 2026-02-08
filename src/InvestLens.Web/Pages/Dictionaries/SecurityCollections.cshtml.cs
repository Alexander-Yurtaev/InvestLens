using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class SecurityCollectionsModel : DictionaryBasePage<SecurityCollectionModel>
{
    public SecurityCollectionsModel(IHttpClientFactory httpFactory, ILogger<SecurityCollectionsModel> logger) : base(httpFactory, logger)
    {
    }

    #region Protected Methods

    public override string Route => "SecurityCollections";
    protected override Type ConcreteType => typeof(SecurityCollectionModelWithPagination);

    #endregion Protected Methods
}