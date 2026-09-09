using System.Linq.Expressions;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Infrastructure.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Domain.Shared.DTOs;
using Domain.Shared.Response;

namespace Infrastructure.Inventory.Repositories;

public class ProductRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : IProductRepository
{
    public async Task CreateAsync(Product entity)
    {
        if (entity.Tags.Any())
        {
            foreach (var tag in entity.Tags)
            {
                context.Tags.Attach(tag);
            }
        }
        await context.Products.AddAsync(entity);
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await context.Products
            .Include(p => p.Tags)
            .Include(p => p.BaseUom)
            .Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Product>> FindAsync(Expression<Func<Product, bool>> predicate)
    {
        return await context.Products.Where(predicate).ToListAsync();
    }

    public void Update(Product entity)
    {
        context.Products.Update(entity);
    }

    public void Delete(Product entity)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResponse<Product>> SearchAsync(SearchProductRequest request)
    {
        var query = context.Products
            .Include(p => p.Category)
            .Include(p => p.BaseUom)
            .AsQueryable();

        if (request.WarehouseId.HasValue && request.WarehouseId > 0)
        {
            query = query.Include(p => p.StockBalances.Where(sb => sb.WarehouseId == request.WarehouseId))
                         .ThenInclude(sb => sb.Warehouse);
        }
        else
        {
            query = query.Include(p => p.StockBalances.Where(sb => sb.Warehouse.IsPrincipal))
                         .ThenInclude(sb => sb.Warehouse);
        }

        query = query.AsNoTracking();

        query = sqlUtils.WhereAnd(
            query, request.CategoryId.HasValue && request.CategoryId > 0,
            p => p.CategoryId == request.CategoryId!.Value
        );

        query = sqlUtils.WhereLike(
            query, !string.IsNullOrWhiteSpace(request.Term), request.Term,
            p => p.Name,
            p => p.Sku
        );

        return await query.OrderByDescending(p => p.Id).ToPagedResponseAsync(request.Page, request.PageSize);
    }
}
