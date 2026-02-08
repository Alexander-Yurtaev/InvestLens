using AutoMapper;
using Grpc.Net.Client;
using InvestLens.Grpc.Service;
using InvestLens.Shared.Models.Dictionaries;

namespace InvestLens.Gateway.Extensions;

public static class DictionariesExtensions
{
    public static RouteHandlerBuilder AddDictionaries(this IEndpointRouteBuilder endpoints, string? dataBaseAddress)
    {
        endpoints.AddEngines(dataBaseAddress);
        endpoints.AddBoards(dataBaseAddress);
        endpoints.AddBoardGroups(dataBaseAddress);
        endpoints.AddMarkets(dataBaseAddress);
        endpoints.AddDurations(dataBaseAddress);
        endpoints.AddSecurityTypes(dataBaseAddress);
        endpoints.AddSecurityGroups(dataBaseAddress);
        return endpoints.AddSecurityCollections(dataBaseAddress);
    }

    private static RouteHandlerBuilder AddEngines(this IEndpointRouteBuilder endpoints, string? dataBaseAddress)
    {
        return endpoints.MapGet("/api/data/engines", async (
            IMapper mapper,
            int page,
            int pageSize,
            string? sort = "",
            string? filter = ""
        ) =>
        {
            try
            {
                // Создаем gRPC клиент
                if (string.IsNullOrEmpty(dataBaseAddress)) throw new InvalidOperationException("Ошибка в настроках.");

                using var channel = GrpcChannel.ForAddress(dataBaseAddress);
                var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

                // Вызываем gRPC метод
                var response = await client.GetEnginesAsync(new GetPaginationRequest()
                    { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });

                // Преобразуем gRPC ответ в REST формат
                var engines = mapper.Map<EngineModelWithPagination>(response);

                return Results.Ok(engines);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error calling data service: {ex.Message}");
            }
        });
    }

    private static RouteHandlerBuilder AddBoards(this IEndpointRouteBuilder endpoints, string? dataBaseAddress)
    {
        return endpoints.MapGet("/api/data/boards", async (
            IMapper mapper,
            int page,
            int pageSize,
            string? sort = "",
            string? filter = ""
        ) =>
        {
            try
            {
                // Создаем gRPC клиент
                if (string.IsNullOrEmpty(dataBaseAddress)) throw new InvalidOperationException("Ошибка в настроках.");

                using var channel = GrpcChannel.ForAddress(dataBaseAddress);
                var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

                // Вызываем gRPC метод
                var response = await client.GetBoardsAsync(new GetPaginationRequest()
                    { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });

                // Преобразуем gRPC ответ в REST формат
                var boards = mapper.Map<BoardModelWithPagination>(response);

                return Results.Ok(boards);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error calling data service: {ex.Message}");
            }
        });
    }

    private static RouteHandlerBuilder AddBoardGroups(this IEndpointRouteBuilder endpoints, string? dataBaseAddress)
    {
        return endpoints.MapGet("/api/data/boardgroups", async (
            IMapper mapper,
            int page,
            int pageSize,
            string? sort = "",
            string? filter = ""
        ) =>
        {
            try
            {
                // Создаем gRPC клиент
                if (string.IsNullOrEmpty(dataBaseAddress)) throw new InvalidOperationException("Ошибка в настроках.");

                using var channel = GrpcChannel.ForAddress(dataBaseAddress);
                var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

                // Вызываем gRPC метод
                var response = await client.GetBoardGroupsAsync(new GetPaginationRequest()
                    { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });

                // Преобразуем gRPC ответ в REST формат
                var boardGroups = mapper.Map<BoardGroupModelWithPagination>(response);

                return Results.Ok(boardGroups);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error calling data service: {ex.Message}");
            }
        });
    }

    private static RouteHandlerBuilder AddMarkets(this IEndpointRouteBuilder endpoints, string? dataBaseAddress)
    {
        return endpoints.MapGet("/api/data/markets", async (
            IMapper mapper,
            int page,
            int pageSize,
            string? sort = "",
            string? filter = ""
        ) =>
        {
            try
            {
                // Создаем gRPC клиент
                if (string.IsNullOrEmpty(dataBaseAddress)) throw new InvalidOperationException("Ошибка в настроках.");

                using var channel = GrpcChannel.ForAddress(dataBaseAddress);
                var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

                // Вызываем gRPC метод
                var response = await client.GetMarketsAsync(new GetPaginationRequest()
                    { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });

                // Преобразуем gRPC ответ в REST формат
                var markets = mapper.Map<MarketModelWithPagination>(response);

                return Results.Ok(markets);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error calling data service: {ex.Message}");
            }
        });
    }

    private static RouteHandlerBuilder AddDurations(this IEndpointRouteBuilder endpoints, string? dataBaseAddress)
    {
        return endpoints.MapGet("/api/data/durations", async (
            IMapper mapper,
            int page,
            int pageSize,
            string? sort = "",
            string? filter = ""
        ) =>
        {
            try
            {
                // Создаем gRPC клиент
                if (string.IsNullOrEmpty(dataBaseAddress)) throw new InvalidOperationException("Ошибка в настроках.");

                using var channel = GrpcChannel.ForAddress(dataBaseAddress);
                var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

                // Вызываем gRPC метод
                var response = await client.GetDurationsAsync(new GetPaginationRequest()
                    { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });

                // Преобразуем gRPC ответ в REST формат
                var durations = mapper.Map<DurationModelWithPagination>(response);

                return Results.Ok(durations);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error calling data service: {ex.Message}");
            }
        });
    }

    private static RouteHandlerBuilder AddSecurityTypes(this IEndpointRouteBuilder endpoints, string? dataBaseAddress)
    {
        return endpoints.MapGet("/api/data/securitytypes", async (
            IMapper mapper,
            int page,
            int pageSize,
            string? sort = "",
            string? filter = ""
        ) =>
        {
            try
            {
                // Создаем gRPC клиент
                if (string.IsNullOrEmpty(dataBaseAddress)) throw new InvalidOperationException("Ошибка в настроках.");

                using var channel = GrpcChannel.ForAddress(dataBaseAddress);
                var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

                // Вызываем gRPC метод
                var response = await client.GetSecurityTypesAsync(new GetPaginationRequest()
                    { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });

                // Преобразуем gRPC ответ в REST формат
                var securityTypes = mapper.Map<SecurityTypeModelWithPagination>(response);

                return Results.Ok(securityTypes);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error calling data service: {ex.Message}");
            }
        });
    }

    private static RouteHandlerBuilder AddSecurityGroups(this IEndpointRouteBuilder endpoints, string? dataBaseAddress)
    {
        return endpoints.MapGet("/api/data/securitygroups", async (
            IMapper mapper,
            int page,
            int pageSize,
            string? sort = "",
            string? filter = ""
        ) =>
        {
            try
            {
                // Создаем gRPC клиент
                if (string.IsNullOrEmpty(dataBaseAddress)) throw new InvalidOperationException("Ошибка в настроках.");

                using var channel = GrpcChannel.ForAddress(dataBaseAddress);
                var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

                // Вызываем gRPC метод
                var response = await client.GetSecurityGroupsAsync(new GetPaginationRequest()
                    { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });

                // Преобразуем gRPC ответ в REST формат
                var securityGroups = mapper.Map<SecurityGroupModelWithPagination>(response);

                return Results.Ok(securityGroups);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error calling data service: {ex.Message}");
            }
        });
    }

    private static RouteHandlerBuilder AddSecurityCollections(this IEndpointRouteBuilder endpoints, string? dataBaseAddress)
    {
        return endpoints.MapGet("/api/data/securitycollections", async (
            IMapper mapper,
            int page,
            int pageSize,
            string? sort = "",
            string? filter = ""
        ) =>
        {
            try
            {
                // Создаем gRPC клиент
                if (string.IsNullOrEmpty(dataBaseAddress)) throw new InvalidOperationException("Ошибка в настроках.");

                using var channel = GrpcChannel.ForAddress(dataBaseAddress);
                var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

                // Вызываем gRPC метод
                var response = await client.GetSecurityCollectionsAsync(new GetPaginationRequest()
                    { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });

                // Преобразуем gRPC ответ в REST формат
                var securityCollections = mapper.Map<SecurityCollectionModelWithPagination>(response);

                return Results.Ok(securityCollections);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error calling data service: {ex.Message}");
            }
        });
    }
}