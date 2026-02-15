using InvestLens.Shared.Helpers;

namespace InvestLens.Data.DataContext;

public class DataContextFactory : BaseDataContextFactory<InvestLensDataContext>
{
    protected override string LocalEnvPath => @"..\InvestLens.Data.Api\.env";
}