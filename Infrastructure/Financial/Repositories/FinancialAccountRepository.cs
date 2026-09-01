using System.Linq.Expressions;
using Domain.Financial.Entities;
using Domain.Financial.Interfaces;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Infrastructure.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Domain.Shared.DTOs;
using Domain.Shared.Response;

namespace Infrastructure.Financial.Repositories;

public class FinancialAccountRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : IFinancialAccountRepository
{
    public async Task CreateAsync(FinancialAccount entity)
    {
        await context.FinancialAccounts.AddAsync(entity);
    }

    public async Task<FinancialAccount?> GetByIdAsync(int id)
    {
        return await context.FinancialAccounts
            .Include(f => f.ChartOfAccounts)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<FinancialAccount>> FindAsync(Expression<Func<FinancialAccount, bool>> predicate)
    {
        return await context.FinancialAccounts
            .Include(f => f.ChartOfAccounts)
            .Where(predicate)
            .ToListAsync();
    }

    public void Update(FinancialAccount entity)
    {
        context.FinancialAccounts.Update(entity);
    }

    public void Delete(FinancialAccount entity)
    {
        context.FinancialAccounts.Remove(entity);
    }

    public async Task<PagedResponse<FinancialAccount>> SearchAsync(SearchFinancialAccountRequest request)
    {
        var query = context.FinancialAccounts
            .Include(f => f.ChartOfAccounts)
            .AsNoTracking();

        query = sqlUtils.WhereAnd(
            query, request.IsActive.HasValue,
            f => f.IsActive == request.IsActive!.Value
        );

        query = sqlUtils.WhereAnd(
            query, request.AccountType.HasValue,
            f => f.AccountType == request.AccountType!.Value
        );

        query = sqlUtils.WhereAnd(
            query, !string.IsNullOrWhiteSpace(request.Name),
            f => EF.Functions.ILike(f.Name, sqlUtils.SqlLikeContains(request.Name!.Trim()))
        );

        return await query
            .OrderByDescending(f => f.Id)
            .ToPagedResponseAsync(request.Page, request.PageSize);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? ignoreId = null)
    {
        var query = context.FinancialAccounts.AsNoTracking().Where(f => f.Name.ToLower() == name.Trim().ToLower());

        if (ignoreId.HasValue)
        {
            query = query.Where(f => f.Id != ignoreId.Value);
        }

        return await query.AnyAsync();
    }
}
