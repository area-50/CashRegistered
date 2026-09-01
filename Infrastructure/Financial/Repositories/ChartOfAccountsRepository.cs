using System.Linq.Expressions;
using Domain.Financial.Entities;
using Domain.Financial.Interfaces;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Infrastructure.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Financial.Repositories;

public class ChartOfAccountsRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : IChartOfAccountsRepository
{
    public async Task CreateAsync(ChartOfAccounts entity)
    {
        await context.ChartOfAccounts.AddAsync(entity);
    }

    public async Task<ChartOfAccounts?> GetByIdAsync(int id)
    {
        return await context.ChartOfAccounts
            .Include(c => c.ParentAccount)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<ChartOfAccounts>> FindAsync(Expression<Func<ChartOfAccounts, bool>> predicate)
    {
        return await context.ChartOfAccounts
            .Include(c => c.ParentAccount)
            .Where(predicate)
            .ToListAsync();
    }

    public void Update(ChartOfAccounts entity)
    {
        context.ChartOfAccounts.Update(entity);
    }

    public void Delete(ChartOfAccounts entity)
    {
        context.ChartOfAccounts.Remove(entity);
    }

    public async Task<PagedResponse<ChartOfAccounts>> SearchAsync(SearchChartOfAccountsRequest request)
    {
        var query = context.ChartOfAccounts
            .Include(c => c.ParentAccount)
            .AsNoTracking();

        query = sqlUtils.WhereAnd(
            query, request.IsActive.HasValue, c => c.IsActive == request.IsActive!.Value
        );
        query = sqlUtils.WhereAnd(
            query, request.AccountType.HasValue, c => c.AccountType == request.AccountType!.Value
        );
        query = sqlUtils.WhereAnd(
            query, request.Nature.HasValue, c => c.Nature == request.Nature!.Value
        );
        query = sqlUtils.WhereAnd(
            query, request.IsSynthetic.HasValue, c => c.IsSynthetic == request.IsSynthetic!.Value
        );
        query = sqlUtils.WhereAnd(
            query, request.AllowPosting.HasValue, c => c.AllowPosting == request.AllowPosting!.Value
        );
        query = sqlUtils.WhereAnd(
            query, !string.IsNullOrWhiteSpace(request.Code),
            c => EF.Functions.ILike(
                c.Code, sqlUtils.SqlLikeContains(request.Code!.Trim())
            )
        );
        query = sqlUtils.WhereAnd(
            query, !string.IsNullOrWhiteSpace(request.Name),
            c => EF.Functions.ILike(
                c.Name, sqlUtils.SqlLikeContains(request.Name!.Trim()))
        );
        
        return await query
            .OrderBy(c => c.Code)
            .ToPagedResponseAsync(request.Page, request.PageSize);
    }

    public async Task<bool> ExistsByCodeAsync(string code, int? ignoreId = null)
    {
        var query = context.ChartOfAccounts.AsNoTracking()
            .Where(c => c.Code.ToLower() == code.Trim().ToLower());

        if (ignoreId.HasValue)
        {
            query = query.Where(c => c.Id != ignoreId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name, int? ignoreId = null)
    {
        var query = context.ChartOfAccounts.AsNoTracking()
            .Where(c => c.Name.ToLower() == name.Trim().ToLower());

        if (ignoreId.HasValue)
        {
            query = query.Where(c => c.Id != ignoreId.Value);
        }

        return await query.AnyAsync();
    }
}
