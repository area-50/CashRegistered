using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Inventory.Request;
using Shared.Response;
using Infrastructure.Common;

namespace Infrastructure.Inventory.Repositories;

public class WarehouseRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : IWarehouseRepository
{
    public async Task CreateAsync(Warehouse entity)
    {
        await context.Warehouses.AddAsync(entity);
    }

    public async Task<Warehouse?> GetByIdAsync(int id)
    {
        return await context.Warehouses
            .Where(w => w.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Warehouse>> FindAsync(System.Linq.Expressions.Expression<Func<Warehouse, bool>> predicate)
    {
        return await context.Warehouses.Where(predicate).ToListAsync();
    }

    public void Update(Warehouse entity)
    {
        context.Warehouses.Update(entity);
    }

    public void Delete(Warehouse entity)
    {
        context.Warehouses.Remove(entity);
    }

    public async Task<PagedResponse<Warehouse>> SearchAsync(SearchWarehouseRequest request)
    {
        var query = context.Warehouses.AsNoTracking();
        
        if (string.IsNullOrWhiteSpace(request.Term))
            return await query.ToPagedResponseAsync(request.Page, request.PageSize);

        var term = sqlUtils.SqlLikeContains(request.Term);

        query = query.Where(w => EF.Functions.ILike(w.Name, term));
    
        return await query.ToPagedResponseAsync(request.Page, request.PageSize);
    }
}
