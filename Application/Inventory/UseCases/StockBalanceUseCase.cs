using Application.Inventory.Interfaces;
using Domain.Inventory.Repositories;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Response;

namespace Application.Inventory.UseCases;

public class StockBalanceUseCase(IStockBalanceRepository repository) : IStockBalanceUseCase
{
    public async Task AddRangeAsync(IEnumerable<Domain.Inventory.Entities.StockBalance> stockBalances)
    {
        await repository.AddRangeAsync(stockBalances);
    }

    public async Task<PagedResponse<GetSearchStockBalanceResponse>> SearchAsync(SearchStockBalanceRequest request)
    {
        return await repository.SearchAsync(request);
    }

    public async Task<decimal> GetAvailableBalanceAsync(int productId, int? warehouseId)
    {
        return await repository.GetAvailableBalanceAsync(productId, warehouseId);
    }
}