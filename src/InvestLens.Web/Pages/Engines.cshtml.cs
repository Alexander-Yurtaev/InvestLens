using InvestLens.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.Reflection;
using InvestLens.Data.Entities.Index;

namespace InvestLens.Web.Pages;

public class EnginesModel : PageModel
{
    private readonly IGlobalDictionariesGrpcClientService _service;
    private readonly ILogger<EnginesModel> _logger;

    public IEnumerable<string> Columns { get; set; } = [];

    public List<Engine> Engines { get; set; } = [];

    // Свойства для пагинации и сортировки
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public string CurrentSort { get; set; } = "";
    public string CurrentFilter { get; set; } = "";
    public Dictionary<string, string> SortColumns { get; set; } = new();

    public EnginesModel(IGlobalDictionariesGrpcClientService service, ILogger<EnginesModel> logger)
    {
        _service = service;
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
        var enginesDto = await _service.GetEnginesAsync(CurrentPage, PageSize, sort, filter);

        if (enginesDto is null)
        {
            _logger.LogWarning($"Метод {nameof(IGlobalDictionariesGrpcClientService.GetEnginesAsync)} не вернул данные.");
        }
        else
        {
            _logger.LogInformation("От gRPC-сервера получено {EnginesCount} записей.", enginesDto.Data.Count);

            Engines = enginesDto.Data;
            TotalPages = enginesDto.TotalPages;
            TotalItems = enginesDto.TotalItems;
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

        return $"/Engines?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}";
    }

    #region Private Methods

    private IEnumerable<string> GetColumns()
    {
        var props = typeof(Engine).GetProperties();
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
            { "Название", "Name" },
            { "описание", "Title" }
        };
    }

    #endregion Private Methods
}