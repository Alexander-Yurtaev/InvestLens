using Prometheus;
using RabbitMQ.Client;
using System.Diagnostics;

namespace InvestLens.Data.Api.Metrics;

public static class RabbitMqMetricsHelper
{
    public static async Task<IConnection> MeasureHealthCheckAsync(
        Func<Task<IConnection>> healthCheckFunc,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        string status = "failure";

        try
        {
            // Инкрементируем общий счетчик
            DataServiceMetrics.RabbitMqHealthCheckCounter.WithLabels("total").Inc();

            var connection = await healthCheckFunc();

            status = "success";
            DataServiceMetrics.RabbitMqHealthCheckCounter.WithLabels("success").Inc();
            DataServiceMetrics.LastRabbitMqHealthCheckTime.SetToCurrentTimeUtc();

            return connection;
        }
        catch (Exception ex)
        {
            DataServiceMetrics.RabbitMqHealthCheckCounter.WithLabels("failure").Inc();

            if (ex is TimeoutException || ex is OperationCanceledException)
            {
                status = "timeout";
            }

            throw;
        }
        finally
        {
            stopwatch.Stop();
            DataServiceMetrics.RabbitMqHealthCheckDuration
                .WithLabels(status)
                .Observe(stopwatch.Elapsed.TotalSeconds);
        }
    }

    // Метод для простого измерения времени
    public static IDisposable StartHealthCheckMeasurement()
    {
        var stopwatch = Stopwatch.StartNew();

        return new DisposableMeasurement(() =>
        {
            stopwatch.Stop();
            DataServiceMetrics.RabbitMqHealthCheckDuration
                .WithLabels("unknown")
                .Observe(stopwatch.Elapsed.TotalSeconds);
        });
    }

    private class DisposableMeasurement : IDisposable
    {
        private readonly Action _onDispose;

        public DisposableMeasurement(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose();
        }
    }
}