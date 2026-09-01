using System.Linq.Expressions;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Domain.Shared.DTOs;
using Domain.Shared.Response;
using Infrastructure.Common;

namespace Infrastructure.Inventory.Repositories;

public class StockBalanceRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : IStockBalanceRepository
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

        query = sqlUtils.WhereAnd(
            query, !string.IsNullOrWhiteSpace(request.Term),
            x => EF.Functions.ILike(x.Product.Name, sqlUtils.SqlLikeContains(request.Term!.Trim())) ||
                 EF.Functions.ILike(x.Product.Sku, sqlUtils.SqlLikeContains(request.Term!.Trim()))
        );

        query = request.WarehouseId.HasValue 
            ? query.Where(x => x.WarehouseId == request.WarehouseId.Value) 
            : query.Where(x => x.Warehouse.IsPrincipal);

        query = sqlUtils.WhereAnd(
            query, request.CategoryId.HasValue,
            x => x.Product.CategoryId == request.CategoryId!.Value
        );

        query = sqlUtils.WhereAnd(
            query, request.HideEmpty == true,
            x => x.AvailableQuantity > 0
        );

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
