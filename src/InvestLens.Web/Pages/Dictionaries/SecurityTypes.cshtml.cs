using InvestLens.Abstraction.Services;
using InvestLens.Data.Entities.Index;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class SecurityTypesModel : DictionaryBasePage<SecurityType>
{
    public SecurityTypesModel(ISecurityTypeDictionariesGrpcClientService service, ILogger<SecurityTypesModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "SecurityTypes";

    #endregion Protected Methods
}