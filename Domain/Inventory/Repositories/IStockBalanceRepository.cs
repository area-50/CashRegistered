using Domain.Inventory.Entities;
using Shared.Abstractions;

namespace Domain.Inventory.Repositories;

public interface IStockBalanceRepository : IRepository<StockBalance>
{
    Task AddRangeAsync(IEnumerable<StockBalance> stockBalances);
}
