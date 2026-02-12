using Prometheus;

namespace InvestLens.Data.Api.Metrics;

public static class DataServiceMetrics
{
    // Время запросов к БД
    public static readonly Histogram DbQueryDuration = Prometheus.Metrics
        .CreateHistogram(
            name: "dataservice_db_query_duration_seconds",
            help: "Duration of database queries",
            labelNames: ["operation", "table"],
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.001, 2, 16)
            }
        );

    // Время обработки gRPC запроса
    public static readonly Histogram GrpcProcessingDuration = Prometheus.Metrics
        .CreateHistogram(
            name: "dataservice_grpc_processing_duration_seconds",
            help: "Duration of gRPC request processing",
            labelNames: ["method", "status"],
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.001, 2, 16)
            }
        );

    // Время выполнения RabbitMQ HealthCheck
    public static readonly Histogram RabbitMqHealthCheckDuration = Prometheus.Metrics
        .CreateHistogram(
            name: "dataservice_rabbitmq_healthcheck_duration_seconds",
            help: "Duration of RabbitMQ health check",
            labelNames: ["status"], // "success", "failure", "timeout"
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.001, 2, 16) // От 1ms до 32s
            }
        );

    // Счетчик количества HealthCheck вызовов
    public static readonly Counter RabbitMqHealthCheckCounter = Prometheus.Metrics
        .CreateCounter(
            name: "dataservice_rabbitmq_healthcheck_total",
            help: "Total number of RabbitMQ health check calls",
            labelNames: ["status"] // "total", "success", "failure"
        );

    // Метрика времени последнего успешного HealthCheck
    public static readonly Gauge LastRabbitMqHealthCheckTime = Prometheus.Metrics
        .CreateGauge(
            name: "dataservice_rabbitmq_healthcheck_last_success_timestamp",
            help: "Unix timestamp of last successful RabbitMQ health check"
        );
}