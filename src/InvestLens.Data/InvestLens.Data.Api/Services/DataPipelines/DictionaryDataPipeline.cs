using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Contracts.Responses;
using InvestLens.Shared.Interfaces.Redis.Services;
using InvestLens.Shared.Interfaces.Services;

namespace InvestLens.Data.Api.Services.DataPipelines;

public abstract class DictionaryDataPipeline<TEntity, TResponse> : DataPipeline<TEntity, TResponse>
    where TEntity : DictionaryBaseEntity
    where TResponse : IBaseDictionaryResponse
{
    protected DictionaryDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<DataPipeline<TEntity, TResponse>> logger,
        int maxConcurrentDownloads = 3, int saveBatchSize = 10000) : base(httpClient, dataWriterService, statusService,
        correlationIdService, logger, maxConcurrentDownloads, saveBatchSize)

    {
    }

    protected override string GetUrl(params int[] args)
    {
        return args[0] != 0 ? string.Empty : $"/iss/index.json";
    }
}