using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class SecurityGroupsModel : DictionaryBasePage<SecurityGroupModel>
{
    public SecurityGroupsModel(ISecurityGroupDictionariesGrpcClient service, ILogger<SecurityGroupsModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "SecurityGroups";

    #endregion Protected Methods
}