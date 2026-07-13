using System.Linq.Expressions;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Response;
using Infrastructure.Common;

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

    public async Task<PagedResponse<GetSearchStockBalanceResponse>> SearchAsync(SearchStockBalanceRequest request)
    {
        var query = context.StockBalances
            .Include(x => x.Product)
            .Include(x => x.Warehouse)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Term))
        {
            var term = request.Term.ToLower();
            query = query.Where(x => 
                x.Product.Name.ToLower().Contains(term) || 
                x.Product.Sku.ToLower().Contains(term)
            );
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.Product.CategoryId == request.CategoryId.Value);
        }

        if (request.HideEmpty == true)
        {
            query = query.Where(x => x.AvailableQuantity > 0);
        }

        return await query
            .OrderBy(x => x.Product.Name)
            .ThenByDescending(x => x.Id)
            .Select(x => new GetSearchStockBalanceResponse
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductSku = x.Product.Sku,
                ProductName = x.Product.Name,
                WarehouseId = x.WarehouseId,
                WarehouseName = x.Warehouse.Name,
                PhysicalQuantity = x.AvailableQuantity + x.ReservedQuantity, // Físico
                ReservedQuantity = x.ReservedQuantity,
                AvailableQuantity = x.AvailableQuantity,
                IsActive = x.IsActive
            })
            .ToPagedResponseAsync(request.Page, request.PageSize);
    }

    public async Task<decimal> GetAvailableBalanceAsync(int productId, int? warehouseId)
    {
        var query = context.StockBalances.AsNoTracking().Where(x => x.ProductId == productId);
        
        if (warehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == warehouseId.Value);
        }
        
        return await query.SumAsync(x => x.AvailableQuantity);
    }
}
