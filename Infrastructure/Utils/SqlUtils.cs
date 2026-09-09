using System.Linq.Expressions;
using Infrastructure.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public IQueryable<T> WhereLike<T>(
        IQueryable<T> query, 
        bool condition, 
        string? term, 
        params Expression<Func<T, string?>>[] propertySelectors)
    {
        if (!condition || string.IsNullOrWhiteSpace(term) || propertySelectors.Length == 0)
            return query;

        var cleanTerm = SqlLikeContains(term.Trim());

        Expression? combinedBody = null;
        var parameter = Expression.Parameter(typeof(T), "x");

        foreach (var selector in propertySelectors)
        {
            var propertyAccess = Expression.Invoke(selector, parameter);
            var iLikeMethod = typeof(NpgsqlDbFunctionsExtensions).GetMethod(nameof(NpgsqlDbFunctionsExtensions.ILike), new[] { typeof(DbFunctions), typeof(string), typeof(string) });
            var efFunctionsProperty = Expression.Property(null, typeof(EF), nameof(EF.Functions));
            var patternArg = Expression.Constant(cleanTerm);

            var iLikeCall = Expression.Call(null, iLikeMethod!, efFunctionsProperty, propertyAccess, patternArg);

            combinedBody = combinedBody == null ? iLikeCall : Expression.OrElse(combinedBody, iLikeCall);
        }

        var lambda = Expression.Lambda<Func<T, bool>>(combinedBody!, parameter);
        return query.Where(lambda);
    }
}
