using System.Linq.Expressions;

namespace Infrastructure.Utils.Interfaces;

public interface ISqlUtils
{
    string SqlLikeContains(string term);
    
    IQueryable<T> WhereAnd<T>(IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate);
    
    IQueryable<T> WhereOr<T>(IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate);
    
    IQueryable<T> WhereLike<T>(IQueryable<T> query, bool condition, string? term, params Expression<Func<T, string?>>[] propertySelectors);
}
