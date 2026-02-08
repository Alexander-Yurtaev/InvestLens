using InvestLens.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace InvestLens.Web.Pages;

public class SecuritiesModel : PageModel
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<SecuritiesModel> _logger;

    public IEnumerable<string> Columns { get; set; } = [];

    public List<SecurityWithDetailsModel> Securities { get; set; } = [];

    // Свойства для пагинации и сортировки
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public string CurrentSort { get; set; } = "";
    public string CurrentFilter { get; set; } = "";
    public Dictionary<string, string> SortColumns { get; set; } = new();

    public SecuritiesModel(IHttpClientFactory httpFactory, ILogger<SecuritiesModel> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
        InitializeSortColumns();
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

        Columns = GetColumns();
        var client = _httpFactory.CreateClient("DataApiClient");
        try
        {
            var response =
                await client.GetAsync($"api/data/securities?page={CurrentPage}&pageSize={PageSize}&sort={sort}&filter={filter}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var model = JsonSerializer.Deserialize<SecurityWithDetailsModelWithPagination>(json, options);

                if (model is null)
                {
                    _logger.LogWarning("Data-сервис не вернул данные.");
                    TempData["Warning"] = "Список ценных бумаг пуст.";
                }
                else
                {
                    _logger.LogInformation("От Data-сервера получено {SecuritiesCount} записей.", model.Models.Count);

                    Securities.AddRange(model.Models);
                    TotalPages = model.TotalPages;
                    TotalItems = model.TotalItems;
                }
            }
            else
            {
                _logger.LogError("Failed to get securities. Status: {StatusCode}", response.StatusCode);
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

        return $"/Securities?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}";
    }

    #region Private Methods

    private IEnumerable<string> GetColumns()
    {
        var props = typeof(SecurityModel).GetProperties();
        foreach (var prop in props)
        {
            var attrs = prop.GetCustomAttributes<DisplayNameAttribute>();
            foreach (var attr in attrs)
            {
                yield return attr.DisplayName;
            }
        }
    }

    private void InitializeSortColumns()
    {
        SortColumns = new Dictionary<string, string>
        {
            { "Код", "SecId" },
            { "Краткое название", "ShortName" },
            { "Полное название", "Name" },
            { "Тип", "Type" },
            { "Группа", "Group" },
            { "Торгуется", "IsTraded" }
        };
    }

    #endregion Private Methods
}