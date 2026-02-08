using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class SecurityTypesModel : DictionaryBasePage<SecurityTypeModel>
{
    public SecurityTypesModel(IHttpClientFactory httpFactory, ILogger<SecurityTypesModel> logger) : base(httpFactory, logger)
    {
    }

    #region Protected Methods

    public override string Route => "SecurityTypes";
    protected override Type ConcreteType => typeof(SecurityTypeModelWithPagination);

    #endregion Protected Methods
}