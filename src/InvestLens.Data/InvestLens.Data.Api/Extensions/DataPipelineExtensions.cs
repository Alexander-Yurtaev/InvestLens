using CorrelationId.HttpClient;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Api.Models.Settings;
using InvestLens.Data.Api.Services;
using Polly;
using Polly.Caching;

namespace InvestLens.Data.Api.Extensions;

public static class DataPipelineExtensions
{
    public static IServiceCollection AddSecurityDataPipeline(this IServiceCollection services, string moexBaseUrl)
    {
        services
            .AddHttpClient<ISecurityDataPipeline, SecurityDataPipeline>(client => client.BaseAddress = new Uri(moexBaseUrl))
            .AddCorrelationIdForwarding()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            })
            .AddPolicyHandler((provider, _) => provider.GetRequiredService<IPollyService>().GetHttpResilientPolicy());

        return services;
    }

    public static IServiceCollection AddIndexDataPipeline(this IServiceCollection services, string moexBaseUrl)
    {
        services
            .AddHttpClient<IEngineDataPipeline, EngineDataPipeline>(client => client.BaseAddress = new Uri(moexBaseUrl))
            .AddCorrelationIdForwarding()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            })
            .AddPolicyHandler((serviceProvider, request) =>
            {
                var cacheProvider = serviceProvider.GetRequiredService<IAsyncCacheProvider>();

                return Policy.CacheAsync<HttpResponseMessage>(
                    cacheProvider,
                    TimeSpan.FromMinutes(5),  // Время жизни кэша
                    onCacheError: (context, key, exception) =>
                    {
                        // Логирование ошибок кэширования
                        var logger = serviceProvider.GetRequiredService<ILogger<EngineDataPipeline>>();
                        logger.LogWarning(exception, "Ошибка кэширования для ключа {CacheKey}", key);
                    }
                );
            })
            .AddPolicyHandler((provider, _) => provider.GetRequiredService<IPollyService>().GetHttpResilientPolicy());

        services
            .AddHttpClient<IMarketDataPipeline, MarketDataPipeline>(client => client.BaseAddress = new Uri(moexBaseUrl))
            .AddCorrelationIdForwarding()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            })
            .AddPolicyHandler((serviceProvider, request) =>
            {
                var cacheProvider = serviceProvider.GetRequiredService<IAsyncCacheProvider>();

                return Policy.CacheAsync<HttpResponseMessage>(
                    cacheProvider,
                    TimeSpan.FromMinutes(5),  // Время жизни кэша
                    onCacheError: (context, key, exception) =>
                    {
                        // Логирование ошибок кэширования
                        var logger = serviceProvider.GetRequiredService<ILogger<EngineDataPipeline>>();
                        logger.LogWarning(exception, "Ошибка кэширования для ключа {CacheKey}", key);
                    }
                );
            })
            .AddPolicyHandler((provider, _) => provider.GetRequiredService<IPollyService>().GetHttpResilientPolicy());

        return services;
    }
}