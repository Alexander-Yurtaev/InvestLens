using Microsoft.Extensions.Configuration;

namespace InvestLens.Abstraction.Services;

public interface IDataService
{
    string GetMasterConnectionString(IConfiguration configuration);

    string GetTargetMasterConnectionString(IConfiguration configuration);

    string GetTargetConnectionString(IConfiguration configuration);

    string GetDatabaseName(IConfiguration configuration);
}