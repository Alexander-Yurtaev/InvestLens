namespace InvestLens.Abstraction.Services;

public interface IDataPipeline
{
    Task<int> ProcessAllDataAsync(Func<Exception, Task> failBack);
}

public interface ISecurityDataPipeline : IDataPipeline
{

}

public interface IEngineDataPipeline : IDataPipeline
{

}

public interface IMarketDataPipeline : IDataPipeline
{

}