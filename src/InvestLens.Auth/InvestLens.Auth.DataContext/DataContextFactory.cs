using InvestLens.Shared.Helpers;

namespace InvestLens.Auth.DataContext;

public class DataContextFactory : BaseDataContextFactory<InvestLensAuthContext>
{
    protected override string LocalEnvPath => @"..\InvestLens.Auth.Api\.env";
}