using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Contracts.Responses;
using InvestLens.Shared.Interfaces.Redis.Services;
using InvestLens.Shared.Interfaces.Services;

namespace InvestLens.Data.Api.Services.DataPipelines;

public class DurationDataPipeline : DictionaryDataPipeline<DurationEntity, DurationDictionaryDataResponse>, IDurationDataPipeline
{
    public DurationDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<DurationDataPipeline> logger) : base(
        httpClient, dataWriterService, statusService, correlationIdService, logger, 1, 100)

    {
    }

    public override string Info => "Available HLOCV candle calculation intervals directory";

    protected override string GetKeyName => "interval";
}