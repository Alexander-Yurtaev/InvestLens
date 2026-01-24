using Hangfire.Common;
using Hangfire.States;
using InvestLens.Worker.Models.Settings;

namespace InvestLens.Worker.Filters;

public class NoRetryForSpecificExceptionsFilter : JobFilterAttribute, IElectStateFilter
{
    private readonly HashSet<Type> _exceptionsToSkip;
    private readonly IJobSettings _jobSettings;
    private readonly ILogger<NoRetryForSpecificExceptionsFilter> _logger;

    public NoRetryForSpecificExceptionsFilter(
        IJobSettings jobSettings,
        Type[] exceptionsToSkip,
        ILogger<NoRetryForSpecificExceptionsFilter> logger)
    {
        _exceptionsToSkip = new HashSet<Type>(exceptionsToSkip);
        _jobSettings = jobSettings;
        _logger = logger;
        Order = -1; // Самый высокий приоритет
    }

    public NoRetryForSpecificExceptionsFilter(
        IJobSettings jobSettings,
        ILogger<NoRetryForSpecificExceptionsFilter> logger) : this(
        jobSettings,
    [
        typeof(MissingMethodException), typeof(MissingMemberException), typeof(TypeLoadException),
        typeof(FileNotFoundException)
    ], logger)
    {

    }

    public void OnStateElection(ElectStateContext context)
    {
        // Пропускаем, если не состояние Failed
        if (context.CandidateState is not FailedState failedState)
            return;

        var exceptionType = failedState.Exception.GetType();
        var exceptionTypeName = exceptionType.FullName;

        _logger.LogDebug(
            "Checking exception {ExceptionType} for job {JobId}",
            exceptionTypeName, context.BackgroundJob.Id);

        // Проверяем, является ли исключение в списке пропускаемых
        // Используем IsAssignableFrom для проверки всей иерархии
        if (_exceptionsToSkip.Any(t => t.IsAssignableFrom(exceptionType)))
        {
            _logger.LogWarning(
                "Job {JobId} failed with {ExceptionType}. No retry will be performed.",
                context.BackgroundJob.Id, exceptionTypeName);

            context.CandidateState = new FailedState(failedState.Exception)
            {
                Reason = $"No retry for {exceptionType.Name}: {failedState.Exception.Message}"
            };

            // Важно: Устанавливаем параметр RetryCount в максимум
            var maxRetryCount = _jobSettings.MaxRetryCount;
            context.SetJobParameter("RetryCount", maxRetryCount);
        }
        else
        {
            _logger.LogDebug(
                "Exception {ExceptionType} is not in skip list. Standard retry logic will be applied.",
                exceptionTypeName);
        }
    }
}