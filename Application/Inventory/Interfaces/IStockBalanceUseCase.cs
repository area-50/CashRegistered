using Domain.Inventory.Entities;
using Domain.Shared.DTOs;
using Domain.Shared.DTOs;
using Domain.Shared.Response;

namespace Application.Inventory.Interfaces;

public interface IStockBalanceUseCase
{
    Task AddRangeAsync(IEnumerable<StockBalance> stockBalances);
    Task<PagedResponse<GetSearchStockBalanceResponse>> SearchAsync(SearchStockBalanceRequest request);
    Task<decimal> GetAvailableBalanceAsync(int productId, int? warehouseId);
    Task ReserveStockAsync(int productId, int warehouseId, decimal quantity);
    Task ReleaseStockReservationAsync(int productId, int warehouseId, decimal quantity);
    Task ConsumeStockReservationAsync(int productId, int warehouseId, decimal quantity);
    Task<StockBalance> GetStockBalanceAsync(int productId, int warehouseId, bool isEntry = false);
    void Update(StockBalance balance);
}