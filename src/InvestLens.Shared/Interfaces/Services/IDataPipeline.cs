namespace InvestLens.Shared.Interfaces.Services;

public interface IDataPipeline
{
    string Info { get; }
    Task<int> ProcessAllDataAsync(Func<Exception, Task> failBack, CancellationToken cancellationToken = default);
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

public interface IBoardDataPipeline : IDataPipeline
{

}

public interface IBoardGroupDataPipeline : IDataPipeline
{

}

public interface IDurationDataPipeline : IDataPipeline
{

}

public interface ISecurityTypeDataPipeline : IDataPipeline
{

}

public interface ISecurityGroupDataPipeline : IDataPipeline
{

}

public interface ISecurityCollectionDataPipeline : IDataPipeline
{

}