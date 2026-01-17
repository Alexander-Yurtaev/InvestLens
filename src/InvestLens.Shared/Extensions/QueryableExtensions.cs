using InvestLens.Data.Entities;

namespace InvestLens.Shared.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> Filter<TEntity, TKey>(this IQueryable<TEntity> query,
        Func<IQueryable<TEntity>, string, IQueryable<TEntity>> whereCause, string? filter)
        where TEntity : BaseEntity<TKey> where TKey : struct
    {
        if (!string.IsNullOrEmpty(filter))
        {
            query = whereCause(query, filter);
        }

        return query;
    }

    public static IQueryable<TEntity> OrderByEx<TEntity, TKey>(this IQueryable<TEntity> query,
        Func<IQueryable<TEntity>, string, IQueryable<TEntity>> sortCause, string? sort)
        where TEntity : BaseEntity<TKey> where TKey : struct
    {
        if (!string.IsNullOrEmpty(sort))
        {
            query = sortCause(query, sort);
        }

        return query;
    }
}