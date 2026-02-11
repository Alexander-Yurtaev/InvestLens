using InvestLens.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.Json.Serialization;
using InvestLens.Shared.Models.Dictionaries;

namespace InvestLens.Web.Pages.Shared;

[JsonDerivedType(typeof(EngineModel))]
public abstract class DictionaryBasePage<TModel> : PageModel
    where TModel : BaseModel
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<DictionaryBasePage<TModel>> _logger;

    public IEnumerable<string> Columns { get; set; } = [];

    public List<TModel> Models { get; set; } = [];

    // Свойства для пагинации и сортировки
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public string CurrentSort { get; set; } = "";
    public string CurrentFilter { get; set; } = "";

    protected DictionaryBasePage(IHttpClientFactory httpFactory, ILogger<DictionaryBasePage<TModel>> logger)
    {
        _httpFactory = httpFactory;
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

        var client = _httpFactory.CreateClient("DataApiClient");

        try
        {
            var response =
                await client.GetAsync($"api/data/{Route.ToLowerInvariant()}?page={CurrentPage}&pageSize={PageSize}&sort={sort}&filter={filter}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                if (JsonSerializer.Deserialize(json, ConcreteType, options) is not BaseModelWithPagination<TModel> model)
                {
                    _logger.LogWarning("The data service did not return the data.");
                    TempData["Warning"] = "Список ценных бумаг пуст.";
                }
                else
                {
                    _logger.LogInformation("{SecuritiesCount} records were received from the Data server.", model.Models.Count);

                    Models.AddRange(model.Models);
                    TotalPages = model.TotalPages;
                    TotalItems = model.TotalItems;
                }
            }
            else
            {
                _logger.LogError("Failed to get {Entities}. Status: {StatusCode}", Route, response.StatusCode);
                TempData["Error"] = $"Не удалось получить {Route}. Статус: {response.StatusCode}";
            }
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

        return $"/Dictionaries/{Route}?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}";
    }

    #region Abstract Methods

    public abstract string Route { get; }
    protected abstract Type ConcreteType { get; }

    #endregion Abstract Methods
}