using Grpc.Core;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InvestLens.Web.Pages;

public abstract class BasePageModel<TModel> : PageModel
    where TModel : BaseModel
{
    private readonly IBaseDictionariesGrpcClient<TModel> _service;
    private readonly ILogger<BasePageModel<TModel>> _logger;

    public IEnumerable<string> Columns { get; set; } = [];

    public List<TModel> Entities { get; set; } = [];

    // Свойства для пагинации и сортировки
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public string CurrentSort { get; set; } = "";
    public string CurrentFilter { get; set; } = "";

    protected BasePageModel(IBaseDictionariesGrpcClient<TModel> service, ILogger<BasePageModel<TModel>> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "size")] int pageSize = 10,
        [FromQuery(Name = "sort")] string? sort = null,
        [FromQuery(Name = "filter")] string? filter = null)
    {
        CurrentPage = page < 1 ? 1 : page;
        PageSize = pageSize < 1 ? 10 : pageSize;
        CurrentSort = sort ?? "";
        CurrentFilter = filter ?? "";

        try
        {
            var entityModels = await _service.GetEntitiesAsync(CurrentPage, PageSize, sort, filter);

            if (entityModels is null)
            {
                _logger.LogWarning("Метод {Method} не вернул данные.", nameof(ISecurityGrpcClient.GetSecuritiesWithDetailsAsync));
                TempData["Warning"] = "Список ценных бумаг пуст.";
            }
            else
            {
                _logger.LogInformation("От gRPC-сервера получено {ModelsCount} записей.", entityModels.Models.Count);

                Entities = entityModels.Models;
                TotalPages = entityModels.TotalPages;
                TotalItems = entityModels.TotalItems;
            }

            return Page();
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, ex.Message);
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            TempData["Error"] = ex.Message;
        }

        return Page();
    }

    public string GetSortClass(string columnName)
    {
        if (string.IsNullOrEmpty(CurrentSort))
            return "";

        if (CurrentSort == columnName)
            return "sort-asc";

        if (CurrentSort == $"{columnName}_desc")
            return "sort-desc";

        return "";
    }

    public string GetSortUrl(string columnName)
    {
        string sortOrder;

        if (CurrentSort == columnName)
            sortOrder = $"{columnName}_desc";
        else if (CurrentSort == $"{columnName}_desc")
            sortOrder = "";
        else
            sortOrder = columnName;

        var queryParams = new Dictionary<string, string>
        {
            { "page", CurrentPage.ToString() },
            { "size", PageSize.ToString() },
            { "filter", CurrentFilter }
        };

        if (!string.IsNullOrEmpty(sortOrder))
            queryParams["sort"] = sortOrder;

        return $"/{Route}?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}";
    }

    #region Protected Methods

    public abstract string Route { get; }

    #endregion Protected Methods
}