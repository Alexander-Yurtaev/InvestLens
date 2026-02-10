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
                Buckets=Histogram.ExponentialBuckets(0.001, 2, 16)
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
                Buckets=Histogram.ExponentialBuckets(0.001, 2, 16)
            }
        );
}