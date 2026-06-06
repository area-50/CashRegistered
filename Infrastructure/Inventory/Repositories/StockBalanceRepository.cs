using System.Linq.Expressions;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Inventory.Repositories;

public class StockBalanceRepository(CashRegisterDbContext context) : IStockBalanceRepository
{
    public async Task CreateAsync(StockBalance entity)
    {
        await context.StockBalances.AddAsync(entity);
    }

    public async Task<StockBalance?> GetByIdAsync(int id)
    {
        return await context.StockBalances.FindAsync(id);
    }

    public async Task<IEnumerable<StockBalance>> FindAsync(Expression<Func<StockBalance, bool>> predicate)
    {
        return await context.StockBalances.Where(predicate).ToListAsync();
    }

    public void Update(StockBalance entity)
    {
        context.StockBalances.Update(entity);
    }

    public void Delete(StockBalance entity)
    {
        context.StockBalances.Remove(entity);
    }

    public async Task AddRangeAsync(IEnumerable<StockBalance> stockBalances)
    {
        await context.StockBalances.AddRangeAsync(stockBalances);
    }
}
