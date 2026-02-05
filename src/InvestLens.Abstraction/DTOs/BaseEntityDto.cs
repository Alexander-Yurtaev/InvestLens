using InvestLens.Data.Entities;

namespace InvestLens.Abstraction.DTOs;

public abstract class BaseEntityDto<TEntity> where TEntity : BaseEntity
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public List<TEntity> Data { get; set; } = [];
    public string? CurrentSort { get; set; }
    public string? CurrentFilter { get; set; }
}