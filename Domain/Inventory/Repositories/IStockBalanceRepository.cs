using Domain.Inventory.Entities;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Response;

namespace Domain.Inventory.Repositories;

public interface IStockBalanceRepository : IRepository<StockBalance>
{
    Task AddRangeAsync(IEnumerable<StockBalance> stockBalances);
    Task<PagedResponse<GetSearchStockBalanceResponse>> SearchAsync(SearchStockBalanceRequest request);
    Task<decimal> GetAvailableBalanceAsync(int productId, int? warehouseId);
}
