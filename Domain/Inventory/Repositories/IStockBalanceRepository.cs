using Domain.Inventory.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.Response;

namespace Domain.Inventory.Repositories;

public interface IStockBalanceRepository : IRepository<StockBalance>
{
    Task AddRangeAsync(IEnumerable<StockBalance> stockBalances);
    
    Task<PagedResponse<GetSearchStockBalanceResponse>> SearchAsync(SearchStockBalanceRequest request);
    
    Task<decimal> GetAvailableBalanceAsync(int productId, int? warehouseId);
}
