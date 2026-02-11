using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Contracts.Responses;
using InvestLens.Shared.Interfaces.Redis.Services;
using InvestLens.Shared.Interfaces.Services;

namespace InvestLens.Data.Api.Services.DataPipelines;

public class SecurityGroupDataPipeline : DictionaryDataPipeline<SecurityGroupEntity, SecurityGroupDictionaryDataResponse>, ISecurityGroupDataPipeline
{
    public SecurityGroupDataPipeline(HttpClient httpClient, IDataWriterService dataWriterService, IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService, ILogger<SecurityGroupDataPipeline> logger) : base(
        httpClient, dataWriterService, statusService, correlationIdService, logger, 1, 100)

    {
    }

    public override string Info => "Trading system instrument groups";
}