using Domain.Inventory.Entities;

namespace Application.Inventory.Interfaces;

public interface IStockBalanceUseCase
{
    Task AddRangeAsync(IEnumerable<StockBalance> stockBalances);
}