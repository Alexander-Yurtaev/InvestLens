using Prometheus;

namespace InvestLens.Gateway.Metrics;

public static class GatewayMetrics
{
    // Время gRPC запроса от Gateway к Data Service
    public static readonly Histogram GrpcRequestDuration = Prometheus.Metrics
        .CreateHistogram(
            name: "gateway_grpc_request_duration_seconds",
            help: "Duration of gRPC requests to Data Service",
            labelNames: ["service", "method", "status"],
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.001, 2, 16)
            }
        );

    // Время обработки в Gateway
    public static readonly Histogram RequestProcessingDuration = Prometheus.Metrics
        .CreateHistogram(
            name: "gateway_request_duration_seconds",
            help: "Duration of request processing in Gateway",
            labelNames: ["route", "status_code"],
            new HistogramConfiguration
            {
                Buckets=Histogram.ExponentialBuckets(0.001, 2, 16)
            }
        );
}