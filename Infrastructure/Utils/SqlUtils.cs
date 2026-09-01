using System.Linq.Expressions;
using Infrastructure.Utils.Interfaces;

namespace Infrastructure.Utils;

public class SqlUtils : ISqlUtils
{
    public string SqlLikeContains(string term)
    {
        return $"%{term}%";
    }

    public IQueryable<T> WhereAnd<T>(IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }

    public IQueryable<T> WhereOr<T>(IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }
}