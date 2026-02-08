namespace InvestLens.Shared.Models;

public abstract class BaseModelWithPagination<TModel> where TModel : BaseModel
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public List<TModel> Models { get; set; } = [];
}