using InvestLens.Data.Entities;

namespace InvestLens.Abstraction.Repositories;

public interface IGetResult<TEntity, TKey> where TEntity : BaseEntity<TKey> where TKey : struct
{
    int Page { get; set; }
    int PageSize { get; set; }
    int TotalPages { get; set; }
    int TotalItems { get; set; }
    List<TEntity> Data { get; set; }
}