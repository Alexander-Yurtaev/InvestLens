using Prometheus;

namespace InvestLens.Web.Metrics;

public static class WebClientMetrics
{
    // Время запроса от Web-клиента к Gateway
    public static readonly Histogram GatewayRequestDuration = Prometheus.Metrics
        .CreateHistogram(
            name: "webclient_gateway_request_duration_seconds",
            help: "Duration of requests from Web-Client to Gateway",
            labelNames: new[] { "method", "endpoint", "status_code" },
            new HistogramConfiguration
            {
                Buckets = [0.001, 0.005, 0.01, 0.05, 0.1, 0.5, 1, 2, 5, 10]
            }
        );

    // Счетчик запросов

    public static readonly Counter GatewayRequestCount = Prometheus.Metrics
        .CreateCounter(
            name: "webclient_gateway_requests_total",
            help: "Total requests to Gateway",
            labelNames: ["method", "endpoint", "status_code"]
        );

    // Активные запросы
    public static readonly Gauge ActiveRequests = Prometheus.Metrics
        .CreateGauge(
            name: "webclient_active_requests",
            help: "Number of active requests"
        );
}