using CorrelationId.HttpClient;
using InvestLens.Data.Api.Services.DataPipelines;
using InvestLens.Shared.Interfaces.Services;

namespace InvestLens.Data.Api.Extensions;

public static class DataPipelineExtensions
{
    public static IServiceCollection AddDataPipeline<TIPipeline, TPipeline>(this IServiceCollection services, string moexBaseUrl)
        where TIPipeline : class
        where TPipeline : class, TIPipeline
    {
        services
            .AddHttpClient<TIPipeline, TPipeline>(client => client.BaseAddress = new Uri(moexBaseUrl))
            .AddCorrelationIdForwarding()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            })
            .AddPolicyHandler((provider, _) => provider.GetRequiredService<IPollyService>().GetHttpResilientPolicy());

        return services;
    }

    public static IServiceCollection AddSecurityDataPipeline(this IServiceCollection services, string moexBaseUrl)
    {
        services.AddDataPipeline<ISecurityDataPipeline, SecurityDataPipeline>(moexBaseUrl);

        return services;
    }

    public static IServiceCollection AddIndexDataPipeline(this IServiceCollection services, string moexBaseUrl)
    {
        services.AddDataPipeline<IEngineDataPipeline, EngineDataPipeline>(moexBaseUrl);
        services.AddDataPipeline<IMarketDataPipeline, MarketDataPipeline>(moexBaseUrl);
        services.AddDataPipeline<IBoardDataPipeline, BoardDataPipeline>(moexBaseUrl);
        services.AddDataPipeline<IBoardGroupDataPipeline, BoardGroupDataPipeline>(moexBaseUrl);
        services.AddDataPipeline<IDurationDataPipeline, DurationDataPipeline>(moexBaseUrl);
        services.AddDataPipeline<ISecurityTypeDataPipeline, SecurityTypeDataPipeline>(moexBaseUrl);
        services.AddDataPipeline<ISecurityGroupDataPipeline, SecurityGroupDataPipeline>(moexBaseUrl);
        services.AddDataPipeline<ISecurityCollectionDataPipeline, SecurityCollectionDataPipeline>(moexBaseUrl);

        return services;
    }
}