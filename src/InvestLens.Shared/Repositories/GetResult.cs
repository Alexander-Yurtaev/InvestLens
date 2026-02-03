using InvestLens.Abstraction.Repositories;
using InvestLens.Data.Entities;

namespace InvestLens.Shared.Repositories;

public class GetResult<TEntity> : IGetResult<TEntity> where TEntity : BaseEntity
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public List<TEntity> Data { get; set; } = [];
}