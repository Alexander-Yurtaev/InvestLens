using AutoMapper;
using Grpc.Net.Client;
using InvestLens.Gateway.Metrics;
using InvestLens.Grpc.Service;
using InvestLens.Shared.Models;

namespace InvestLens.Gateway.Extensions;

public static class SecuritiesExtensions
{
    public static RouteHandlerBuilder AddSecurities(this IEndpointRouteBuilder endpoints, string? dataBaseAddress)
    {
        return endpoints.MapGet("/api/data/securities", async (
            IMapper mapper,
            int page=1,
            int pageSize=10,
            string? sort = "",
            string? filter = ""
        ) =>
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Создаем gRPC клиент
                if (string.IsNullOrEmpty(dataBaseAddress)) throw new InvalidOperationException("Configuration error.");

                using var channel = GrpcChannel.ForAddress(dataBaseAddress);
                var client = new SecurityServices.SecurityServicesClient(channel);

                // Вызываем gRPC метод
                var response = await client.GetSecuritiesWithDetailsAsync(new GetPaginationRequest()
                    { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });

                // Преобразуем gRPC ответ в REST формат
                var securities = mapper.Map<SecurityWithDetailsModelWithPagination>(response);

                GatewayMetrics.GrpcRequestDuration
                    .WithLabels("DataService", "GetSecurities", "OK")
                    .Observe(stopwatch.Elapsed.TotalSeconds);

                return Results.Ok(securities);
            }
            catch (Exception ex)
            {
                GatewayMetrics.GrpcRequestDuration
                    .WithLabels("DataService", "GetSecurities", "ERROR")
                    .Observe(stopwatch.Elapsed.TotalSeconds);

                return Results.Problem($"Error calling data service: {ex.Message}");
            }
        })
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get securities list with details";
            operation.Description = "Returns a paginated list of securities with detailed information";
            return operation;
        })
        .WithTags("Securities")
        .Produces<SecurityWithDetailsModelWithPagination>()
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}