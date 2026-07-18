using System.Linq.Expressions;
using Domain.Inventory.Entities;
using Domain.Inventory.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using Shared.Response;
using Shared.Inventory.Request;

namespace Infrastructure.Inventory.Repositories;

public class CostCenterRepository(CashRegisterDbContext context) : ICostCenterRepository
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
        
        if (!string.IsNullOrWhiteSpace(request.Name))
            query = query.Where(c => c.Name.ToLower().Contains(request.Name.ToLower()));
            
        if (request.IsActive.HasValue)
            query = query.Where(c => c.IsActive == request.IsActive.Value);

        return await query
            .OrderByDescending(c => c.Id)
            .ToPagedResponseAsync(request.Page, request.PageSize);
    }
}
