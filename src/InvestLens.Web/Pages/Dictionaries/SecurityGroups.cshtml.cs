using InvestLens.Shared.Models.Dictionaries;
using InvestLens.Web.Pages.Shared;

namespace InvestLens.Web.Pages.Dictionaries;

public class SecurityGroupsModel : DictionaryBasePage<SecurityGroupModel>
{
    public SecurityGroupsModel(IHttpClientFactory httpFactory, ILogger<SecurityGroupsModel> logger) : base(httpFactory, logger)
    {
    }

    #region Protected Methods

    public override string Route => "SecurityGroups";
    protected override Type ConcreteType => typeof(SecurityGroupModelWithPagination);

    #endregion Protected Methods
}