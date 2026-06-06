using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;

namespace Application.Inventory.UseCases;

public class StockBalanceUseCase(
    IStockBalanceRepository repository
) : IStockBalanceUseCase
{
    public async Task AddRangeAsync(IEnumerable<StockBalance> stockBalances)
    {
        await repository.AddRangeAsync(stockBalances);
    }
}