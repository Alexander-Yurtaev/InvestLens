using InvestLens.Abstraction.Services;
using InvestLens.Data.DataContext;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace InvestLens.Data.Api.Extensions;

public static class InvestLensDataContextExtension
{
    public static IServiceCollection AddInvestLensDatabaseInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Регистрируем DbContext
        var connectionString = PostgresDataHelper.GetTargetConnectionString(configuration);
        services.AddDbContext<InvestLensDataContext>(options =>
            {
                options.UseNpgsql(connectionString);
            },
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Transient);

        // Регистрируем DatabaseService
        services.AddScoped<IDatabaseService, DatabaseService<InvestLensDataContext>>();

        return services;
    }

}