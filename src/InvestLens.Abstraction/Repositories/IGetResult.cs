namespace InvestLens.Abstraction.Repositories;

public interface IGetResult<TEntity> where TEntity : class
{
    int Page { get; set; }
    int PageSize { get; set; }
    int TotalPages { get; set; }
    int TotalItems { get; set; }
    List<TEntity> Data { get; set; }
}