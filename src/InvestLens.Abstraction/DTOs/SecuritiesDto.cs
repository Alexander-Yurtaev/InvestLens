using InvestLens.Data.Entities;

namespace InvestLens.Abstraction.DTOs;

public class SecuritiesDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public List<Security> Data { get; set; } = [];
    public string? CurrentSort { get; set; }
    public string? CurrentFilter { get; set; }
}