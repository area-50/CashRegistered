using System.Linq.Expressions;
using Domain.Financial.Entities;
using Domain.Financial.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Common;
using Infrastructure.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Domain.Shared.Abstractions;
using Domain.Shared.Response;
using Domain.Shared.DTOs;

namespace Infrastructure.Financial.Repositories;

public class CostCenterRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : ICostCenterRepository
{
    public async Task CreateAsync(CostCenter entity) => await context.CostCenters.AddAsync(entity);

    public async Task<CostCenter?> GetByIdAsync(int id) => await context.CostCenters
        .Include(c => c.Manager)
            .ThenInclude(m => m.Person)
        .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<CostCenter>> FindAsync(Expression<Func<CostCenter, bool>> predicate) =>
        await context.CostCenters
            .Include(c => c.Manager)
                .ThenInclude(m => m.Person)
            .Where(predicate)
            .ToListAsync();

    public void Update(CostCenter entity) => context.CostCenters.Update(entity);

    public void Delete(CostCenter entity) => context.CostCenters.Remove(entity);

    public async Task<PagedResponse<CostCenter>> SearchAsync(SearchCostCenterRequest request)
    {
        var query = context.CostCenters
            .Include(c => c.Manager)
                .ThenInclude(m => m.Person)
            .AsNoTracking();
        
        query = sqlUtils.WhereAnd(
            query, !string.IsNullOrWhiteSpace(request.Name),
            c => EF.Functions.ILike(c.Name, sqlUtils.SqlLikeContains(request.Name!.Trim()))
        );

        query = sqlUtils.WhereAnd(
            query, request.IsActive.HasValue,
            c => c.IsActive == request.IsActive!.Value
        );

        return await query
            .OrderByDescending(c => c.Id)
            .ToPagedResponseAsync(request.Page, request.PageSize);
    }
}

