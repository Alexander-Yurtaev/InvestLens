using InvestLens.Abstraction.Services;
using InvestLens.Data.Entities.Index;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class SecurityGroupsModel : DictionaryBasePage<SecurityGroup>
{
    public SecurityGroupsModel(ISecurityGroupDictionariesGrpcClientService service, ILogger<SecurityGroupsModel> logger) : base(service, logger)
    {
    }

    #region Protected Methods

    public override string Route => "SecurityGroups";

    #endregion Protected Methods
}