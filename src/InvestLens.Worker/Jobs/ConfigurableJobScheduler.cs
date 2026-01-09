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
        _jobsConfig = jobsConfig.Value ?? new HangfireJobsConfiguration();
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Регистрация всех повторяющихся задач из конфигурации
    /// </summary>
    public void ScheduleRecurringJobs()
    {
        _logger.LogInformation("Начинаю регистрацию повторяющихся задач из конфигурации...");

        foreach (var jobConfig in _jobsConfig.RecurringJobs)
        {
            if (!jobConfig.Enabled)
            {
                _logger.LogInformation("Задача {JobId} отключена в конфигурации", jobConfig.JobId);
                continue;
            }

            try
            {
                RegisterRecurringJob(jobConfig);
                _logger.LogInformation("Зарегистрирована задача {JobId}: {Description} ({Cron})",
                    jobConfig.JobId, jobConfig.Description, jobConfig.CronExpression);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при регистрации задачи {JobId}", jobConfig.JobId);
            }
        }
    }

    /// <summary>
    /// Регистрация задач при старте приложения
    /// </summary>
    public void ScheduleStartupJobs()
    {
        _logger.LogInformation("Планирую задачи при старте приложения...");

        foreach (var jobConfig in _jobsConfig.StartupJobs)
        {
            if (!jobConfig.Enabled)
            {
                _logger.LogInformation("Задача при старте {JobId} отключена", jobConfig.JobId);
                continue;
            }

            try
            {
                ScheduleStartupJob(jobConfig);
                _logger.LogInformation("Запланирована задача при старте {JobId}", jobConfig.JobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при планировании задачи при старте {JobId}", jobConfig.JobId);
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
            _logger.LogWarning("Задача {JobId} не найдена в конфигурации", jobId);
            return;
        }

        jobConfig.CronExpression = newCronExpression;

        // Перерегистрируем задачу с новым расписанием
        RecurringJob.AddOrUpdate(
            jobConfig.JobId,
            jobConfig.Queue,
            () => ExecuteJob(jobConfig.ServiceType, jobConfig.MethodName),
            () => jobConfig.CronExpression,
            new RecurringJobOptions
            {
                TimeZone = jobConfig.GetTimeZoneInfo()
            });

        _logger.LogInformation("Обновлено расписание задачи {JobId}: {Cron}", jobId, newCronExpression);
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
            () => ExecuteJob(jobConfig.ServiceType, jobConfig.MethodName),
            () => jobConfig.CronExpression,
            new RecurringJobOptions
            {
                TimeZone = jobConfig.GetTimeZoneInfo()
            });
    }

    private void ScheduleStartupJob(StartupJobConfig config)
    {
        BackgroundJob.Schedule(
            () => ExecuteJob(config.ServiceType, config.MethodName),
            TimeSpan.FromSeconds(config.DelaySeconds));
    }

    /// <summary>
    /// Динамическое выполнение задачи по ее конфигурации
    /// </summary>
    [JobDisplayName("{0}")]
    public async Task ExecuteJob(string serviceTypeName, string methodName)
    {
        try
        {
            _logger.LogDebug("Запуск задачи: {ServiceType}.{MethodName}", serviceTypeName, methodName);

            // Получаем тип сервиса
            var serviceType = Type.GetType(serviceTypeName);
            if (serviceType == null)
            {
                _logger.LogError("Тип сервиса не найден: {ServiceType}", serviceTypeName);
                throw new MissingMemberException($"Тип сервиса не найден: {serviceTypeName}");
            }

            // Получаем сервис из DI контейнера
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetService(serviceType);
            if (service == null)
            {
                _logger.LogError("Сервис не зарегистрирован в DI: {ServiceType}", serviceTypeName);
                throw new MissingMemberException($"Сервис не зарегистрирован в DI: {serviceTypeName}");
            }

            // Находим метод
            var method = serviceType.GetMethod(methodName);
            if (method == null)
            {
                _logger.LogError("Метод не найден: {MethodName} в {ServiceType}", methodName, serviceTypeName);
                throw new MissingMethodException($"Метод не найден: {methodName} в {serviceTypeName}");
            }

            // Вызываем метод
            var result = method.Invoke(service, null);

            // Если метод асинхронный, ждем завершения
            if (result is Task task)
            {
                await task;
            }

            _logger.LogDebug("Задача выполнена успешно: {ServiceType}.{MethodName}",
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
                "Ошибка в вызванном методе: {ServiceType}.{MethodName}",
                serviceTypeName, methodName);
            throw ex.InnerException ?? ex;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении задачи: {ServiceType}.{MethodName}",
                serviceTypeName, methodName);
            throw;
        }
    }

    #endregion
}