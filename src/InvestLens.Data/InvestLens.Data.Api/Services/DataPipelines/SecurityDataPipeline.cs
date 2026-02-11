using InvestLens.Data.Entities;
using InvestLens.Shared.Contracts.Responses;
using InvestLens.Shared.Interfaces.Redis.Services;
using InvestLens.Shared.Interfaces.Services;

namespace InvestLens.Data.Api.Services.DataPipelines;

public class SecurityDataPipeline : DataPipeline<SecurityEntity, SecuritiesResponse>, ISecurityDataPipeline
{
    public SecurityDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<DataPipeline<SecurityEntity, SecuritiesResponse>> logger) : base(
        httpClient, dataWriterService, statusService, correlationIdService, logger)

    {
    }

    public override string Info => "Securities list";
    protected override string GetKeyName => "secid";

    protected override string GetUrl(params int[] args)
    {
        var page = args[0];
        var batchSize = args[1];

        return $"/iss/securities.json?start={page * batchSize}&limit={batchSize}";
    }
}