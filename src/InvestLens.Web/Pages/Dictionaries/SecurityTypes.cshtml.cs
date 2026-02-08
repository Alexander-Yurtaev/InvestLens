using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class SecurityTypesModel : DictionaryBasePage<SecurityTypeModel>
{
    public SecurityTypesModel(ISecurityTypeDictionariesGrpcClient service, ILogger<SecurityTypesModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "SecurityTypes";

    #endregion Protected Methods
}