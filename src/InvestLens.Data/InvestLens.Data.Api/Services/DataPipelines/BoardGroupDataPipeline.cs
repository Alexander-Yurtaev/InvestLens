using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Contracts.Responses;
using InvestLens.Shared.Interfaces.Redis.Services;
using InvestLens.Shared.Interfaces.Services;

namespace InvestLens.Data.Api.Services.DataPipelines;

public class BoardGroupDataPipeline : DictionaryDataPipeline<BoardGroupEntity, BoardGroupDictionaryDataResponse>, IBoardGroupDataPipeline
{
    public BoardGroupDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<BoardGroupDataPipeline> logger) : base(
        httpClient, dataWriterService, statusService, correlationIdService, logger, 1, 100)

    {
    }

    public override string Info => "Trading mode groups directory";
}