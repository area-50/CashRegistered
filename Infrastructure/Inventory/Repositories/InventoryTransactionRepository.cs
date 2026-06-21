using System.Linq.Expressions;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
}
