using InvestLens.Abstraction.Services;
using InvestLens.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InvestLens.Web.Pages;

public abstract class BaseModel<TEntity> : PageModel
    where TEntity : BaseEntity
{
    private readonly IBaseDictionariesGrpcClientService<TEntity> _service;
    private readonly ILogger<BaseModel<TEntity>> _logger;

    public IEnumerable<string> Columns { get; set; } = [];

    public List<TEntity> Entities { get; set; } = [];

    // Свойства для пагинации и сортировки
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public string CurrentSort { get; set; } = "";
    public string CurrentFilter { get; set; } = "";

    protected BaseModel(IBaseDictionariesGrpcClientService<TEntity> service, ILogger<BaseModel<TEntity>> logger)
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

        var entitiesDto = await _service.GetEntitiesAsync(CurrentPage, PageSize, sort, filter);

        if (entitiesDto is null)
        {
            _logger.LogWarning($"Метод {nameof(IBaseDictionariesGrpcClientService<TEntity>.GetEntitiesAsync)} не вернул данные.");
        }
        else
        {
            _logger.LogInformation("От gRPC-сервера получено {EntitiesCount} записей.", entitiesDto.Data.Count);

            Entities = entitiesDto.Data;
            TotalPages = entitiesDto.TotalPages;
            TotalItems = entitiesDto.TotalItems;
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