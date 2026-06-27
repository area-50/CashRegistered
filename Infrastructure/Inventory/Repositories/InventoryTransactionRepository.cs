using System.Linq.Expressions;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Response;
using Shared.Inventory.Request;
using Infrastructure.Common;

namespace Infrastructure.Inventory.Repositories;

public class InventoryTransactionRepository(CashRegisterDbContext context) : IInventoryTransactionRepository
{
    public async Task CreateAsync(InventoryTransaction entity)
    {
        await context.InventoryTransactions.AddAsync(entity);
    }

    public async Task<InventoryTransaction?> GetByIdAsync(int id)
    {
        return await context.InventoryTransactions
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .Include(x => x.Items)
                .ThenInclude(i => i.SourceWarehouse)
            .Include(x => x.Items)
                .ThenInclude(i => i.DestinationWarehouse)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<InventoryTransaction>> FindAsync(Expression<Func<InventoryTransaction, bool>> predicate)
    {
        return await context.InventoryTransactions
            .Include(x => x.Items)
            .Where(predicate)
            .ToListAsync();
    }

    public void Update(InventoryTransaction entity)
    {
        context.InventoryTransactions.Update(entity);
    }

    public void Delete(InventoryTransaction entity)
    {
        context.InventoryTransactions.Remove(entity);
    }

    public async Task<PagedResponse<InventoryTransaction>> SearchAsync(SearchInventoryTransactionRequest request)
    {
        var query = context.InventoryTransactions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.ReferenceDocument))
        {
            var term = request.ReferenceDocument.ToLower();
            query = query.Where(x => x.ReferenceDocument != null && x.ReferenceDocument.ToLower().Contains(term));
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.DateTime >= request.StartDate.Value.ToUniversalTime());
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.DateTime <= request.EndDate.Value.ToUniversalTime());
        }

        return await query
            .OrderByDescending(x => x.DateTime)
            .ToPagedResponseAsync(request.Page, request.PageSize);
    }
}
