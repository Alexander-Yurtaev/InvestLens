using Hangfire;
using InvestLens.Worker.Models;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace InvestLens.Worker.Jobs;

public class ConfigurableJobScheduler : IConfigurableJobScheduler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConfigurableJobScheduler> _logger;
    private readonly HangfireJobsConfiguration _jobsConfig;

    public ConfigurableJobScheduler(
        IOptions<HangfireJobsConfiguration> jobsConfig,
        IServiceProvider serviceProvider,
        ILogger<ConfigurableJobScheduler> logger)
    {
        _jobsConfig = jobsConfig.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Регистрация всех повторяющихся задач из конфигурации
    /// </summary>
    public void ScheduleRecurringJobs()
    {
        _logger.LogInformation("I'm starting to register recurring tasks from the configuration...");

        foreach (var jobConfig in _jobsConfig.RecurringJobs)
        {
            if (!jobConfig.Enabled)
            {
                _logger.LogInformation("The {JobId} task is disabled in the configuration", jobConfig.JobId);
                continue;
            }

            try
            {
                RegisterRecurringJob(jobConfig);
                _logger.LogInformation("Task registered {JobID}: {Description} ({Cron})",
                    jobConfig.JobId, jobConfig.Description, jobConfig.CronExpression);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Issue registration error {JobId}", jobConfig.JobId);
            }
        }
    }

    /// <summary>
    /// Регистрация задач при старте приложения
    /// </summary>
    public void ScheduleStartupJobs()
    {
        _logger.LogInformation("I plan tasks at the start of the application...");

        foreach (var jobConfig in _jobsConfig.StartupJobs)
        {
            if (!jobConfig.Enabled)
            {
                _logger.LogInformation("The {JobId} task is disabled at startup", jobConfig.JobId);
                continue;
            }

            try
            {
                ScheduleStartupJob(jobConfig);
                _logger.LogInformation("Scheduled task at startup {JobId}", jobConfig.JobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when scheduling a task at startup {JobId}", jobConfig.JobId);
            }
        }
    }

    /// <summary>
    /// Обновление расписания существующей задачи
    /// </summary>
    public void UpdateJobSchedule(string jobId, string newCronExpression)
    {
        var jobConfig = _jobsConfig.RecurringJobs.FirstOrDefault(j => j.JobId == jobId);
        if (jobConfig == null)
        {
            _logger.LogWarning("The {JobId} task was not found in the configuration", jobId);
            return;
        }

        jobConfig.CronExpression = newCronExpression;

        // Перерегистрируем задачу с новым расписанием
        RecurringJob.AddOrUpdate(
            jobConfig.JobId,
            jobConfig.Queue,
            () => ExecuteJob(jobConfig.JobId, jobConfig.ServiceType, jobConfig.MethodName),
            () => jobConfig.CronExpression,
            new RecurringJobOptions
            {
                TimeZone = jobConfig.GetTimeZoneInfo()
            });

        _logger.LogInformation("Task schedule updated {JobId}: {Cron}", jobId, newCronExpression);
    }

    /// <summary>
    /// Получение списка зарегистрированных задач
    /// </summary>
    public List<RecurringJobConfig> GetRegisteredJobs()
    {
        return _jobsConfig.RecurringJobs
            .Where(j => j.Enabled)
            .ToList();
    }

    #region Private Methods

    private void RegisterRecurringJob(RecurringJobConfig jobConfig)
    {
        // Используем RecurringJob.AddOrUpdate с лямбдой, вызывающей наш метод
        RecurringJob.AddOrUpdate(
            jobConfig.JobId,
            jobConfig.Queue,
            () => ExecuteJob(jobConfig.JobId, jobConfig.ServiceType, jobConfig.MethodName),
            () => jobConfig.CronExpression,
            new RecurringJobOptions
            {
                TimeZone = jobConfig.GetTimeZoneInfo()
            });
    }

    private void ScheduleStartupJob(StartupJobConfig jobConfig)
    {
        BackgroundJob.Schedule(
            () => ExecuteJob(jobConfig.JobId, jobConfig.ServiceType, jobConfig.MethodName),
            TimeSpan.FromSeconds(jobConfig.DelaySeconds));
    }

    /// <summary>
    /// Динамическое выполнение задачи по ее конфигурации
    /// </summary>
    [JobDisplayName("{0}")]
    public async Task ExecuteJob(string jobId, string serviceTypeName, string methodName)
    {
        try
        {
            _logger.LogDebug("Starting a task: {ServiceType}.{MethodName}", serviceTypeName, methodName);

            // Получаем тип сервиса
            var serviceType = Type.GetType(serviceTypeName);
            if (serviceType == null)
            {
                _logger.LogError("Service type not found: {ServiceType}", serviceTypeName);
                throw new MissingMemberException($"Service type not found: {serviceTypeName}");
            }

            // Получаем сервис из DI контейнера
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetService(serviceType);
            if (service == null)
            {
                _logger.LogError("The service is not registered in DI: {ServiceType}", serviceTypeName);
                throw new MissingMemberException($"The service is not registered in DI: {serviceTypeName}");
            }

            // Находим метод
            var method = serviceType.GetMethod(methodName);
            if (method == null)
            {
                _logger.LogError("Method not found: {MethodName} в {ServiceType}", methodName, serviceTypeName);
                throw new MissingMethodException($"Method not found: {methodName} в {serviceTypeName}");
            }

            // Вызываем метод
            var result = method.Invoke(service, null);

            // Если метод асинхронный, ждем завершения
            if (result is Task task)
            {
                await task;
            }

            _logger.LogDebug("The task was completed successfully: {ServiceType}.{MethodName}",
                serviceTypeName, methodName);
        }
        catch (MissingMethodException)
        {
            throw;
        }
        catch (MissingMemberException)
        {
            throw;
        }
        catch (TargetInvocationException ex)
        {
            _logger.LogError(ex.InnerException ?? ex,
                "Error in the called method: {ServiceType}.{MethodName}",
                serviceTypeName, methodName);
            throw ex.InnerException ?? ex;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when completing the task: {ServiceType}.{MethodName}",
                serviceTypeName, methodName);
            throw;
        }
    }

    #endregion
}