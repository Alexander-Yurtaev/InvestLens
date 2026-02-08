using InvestLens.Data.Entities;

namespace InvestLens.Data.Repositories.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> Filter<TEntity>(this IQueryable<TEntity> query,
        Func<IQueryable<TEntity>, string, IQueryable<TEntity>> whereCause, string? filter)
        where TEntity : BaseEntity
    {
        if (!string.IsNullOrEmpty(filter))
        {
            query = whereCause(query, filter);
        }

        return query;
    }

    public static IEnumerable<TEntity> OrderByEx<TEntity>(this IEnumerable<TEntity> query,
        Func<IEnumerable<TEntity>, string, IEnumerable<TEntity>> sortCause, string? sort)
        where TEntity : BaseEntity
    {
        if (!string.IsNullOrEmpty(sort))
        {
            query = sortCause(query, sort);
        }

        return query;
    }
}