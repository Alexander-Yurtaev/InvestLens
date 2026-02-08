namespace InvestLens.Shared.Contracts.Dto.Pagination;

public abstract class BaseDtoWithPagination<TEntity> where TEntity : BaseDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public List<TEntity> Data { get; set; } = [];
    public string? CurrentSort { get; set; }
    public string? CurrentFilter { get; set; }
}