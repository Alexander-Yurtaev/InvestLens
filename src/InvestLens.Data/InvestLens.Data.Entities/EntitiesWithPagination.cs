namespace InvestLens.Data.Entities;

public class EntitiesWithPagination<TEntity> where TEntity : BaseEntity
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public List<TEntity> Entities { get; set; } = [];
}