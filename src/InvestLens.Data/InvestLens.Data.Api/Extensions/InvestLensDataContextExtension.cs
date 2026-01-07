using InvestLens.Data.DataContext;
using InvestLens.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace InvestLens.Data.Api.Extensions;

public static class InvestLensDataContextExtension
{
    public static IServiceCollection AddInvestLensDatabaseInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Регистрируем DbContext
        var connectionString = ConnectionStringHelper.GetTargetConnectionString(configuration);
        services.AddDbContext<InvestLensDataContext>(options =>
            {
                options.UseNpgsql(connectionString);
            },
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Transient);

        return services;
    }
}