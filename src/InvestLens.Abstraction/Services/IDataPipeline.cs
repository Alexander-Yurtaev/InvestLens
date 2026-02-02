namespace InvestLens.Abstraction.Services;

public interface IDataPipeline
{
    Task<int> ProcessAllDataAsync(Func<Exception, Task> failBack);
}