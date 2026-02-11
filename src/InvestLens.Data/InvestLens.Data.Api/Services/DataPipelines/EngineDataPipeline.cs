using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Contracts.Responses;
using InvestLens.Shared.Interfaces.Redis.Services;
using InvestLens.Shared.Interfaces.Services;

namespace InvestLens.Data.Api.Services.DataPipelines;

public class EngineDataPipeline : DictionaryDataPipeline<EngineEntity, EngineDictionaryDataResponse>, IEngineDataPipeline
{
    public EngineDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<EngineDataPipeline> logger) : base(
        httpClient, dataWriterService, statusService, correlationIdService, logger, 1, 100)

    {
    }

    public override string Info => "Available trading systems list";
}