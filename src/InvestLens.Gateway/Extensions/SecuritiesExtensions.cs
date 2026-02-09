using AutoMapper;
using Grpc.Net.Client;
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
            try
            {
                // Создаем gRPC клиент
                if (string.IsNullOrEmpty(dataBaseAddress)) throw new InvalidOperationException("Ошибка в настроках.");

                using var channel = GrpcChannel.ForAddress(dataBaseAddress);
                var client = new SecurityServices.SecurityServicesClient(channel);

                // Вызываем gRPC метод
                var response = await client.GetSecuritiesWithDetailsAsync(new GetPaginationRequest()
                    { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });

                // Преобразуем gRPC ответ в REST формат
                var securities = mapper.Map<SecurityWithDetailsModelWithPagination>(response);

                return Results.Ok(securities);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error calling data service: {ex.Message}");
            }
        })
        .WithOpenApi(operation =>
        {
            operation.Summary = "Получить список ценных бумаг с деталями";
            operation.Description = "Возвращает пагинированный список ценных бумаг с детальной информацией";
            return operation;
        })
        .WithTags("Securities")
        .Produces<SecurityWithDetailsModelWithPagination>()
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}