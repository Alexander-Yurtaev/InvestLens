using InvestLens.Abstraction.Services;
using InvestLens.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.Reflection;

namespace InvestLens.Web.Pages;

public class SecuritiesModel : PageModel
{
    private readonly ISecurityGrpcClientService _service;
    private readonly ILogger<SecuritiesModel> _logger;

    public IEnumerable<string> Columns { get; set; }

    public List<Security> Securities { get; set; }

    public SecuritiesModel(ISecurityGrpcClientService service, ILogger<SecuritiesModel> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet(
        [FromQuery(Name = "p")] int page,
        [FromQuery(Name = "size")] int pageSize,
        [FromQuery(Name = "sort")] string? sort,
        [FromQuery(Name = "filter")] string? filter
        )
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        Columns = GetColumns();
        Securities = (await _service.GetSecuritiesAsync(page, pageSize, sort, filter)).ToList();

        return Page();
    }

    #region Private Methods

    private IEnumerable<string> GetColumns()
    {
        var props = typeof(Security).GetProperties();
        foreach (var prop in props)
        {
            var attrs = prop.GetCustomAttributes<DisplayNameAttribute>();
            foreach (var attr in attrs)
            {
                yield return attr.DisplayName;
            }
        }
    }

    #endregion Private Methods
}