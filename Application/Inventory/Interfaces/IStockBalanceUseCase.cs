using Domain.Inventory.Entities;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Response;

namespace Application.Inventory.Interfaces;

public interface IStockBalanceUseCase
{
    Task AddRangeAsync(IEnumerable<StockBalance> stockBalances);
    Task<PagedResponse<GetSearchStockBalanceResponse>> SearchAsync(SearchStockBalanceRequest request);
    Task<decimal> GetAvailableBalanceAsync(int productId, int? warehouseId);
}