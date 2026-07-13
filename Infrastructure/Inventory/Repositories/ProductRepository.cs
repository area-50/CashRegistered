using System.Linq.Expressions;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Infrastructure.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Inventory.Request;
using Shared.Response;

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

    public async  Task<PagedResponse<Product>> SearchAsync(SearchProductRequest request)
    {
        var query = context.Products
            .Include(p => p.Category)
            .Include(p => p.BaseUom)
            .AsNoTracking();
        
        if (string.IsNullOrWhiteSpace(request.Term) && request.CategoryId is null or 0)
            return await query.OrderByDescending(p => p.Id).ToPagedResponseAsync(request.Page, request.PageSize);

        var term = sqlUtils.SqlLikeContains(request.Term!);
        var categoryId = request.CategoryId ?? 0;

        query = query.Where(p =>
                p.CategoryId == categoryId &&
                (EF.Functions.ILike(p.Name, term) || EF.Functions.ILike(p.Sku, term))
        );
    
        return await query.OrderByDescending(p => p.Id).ToPagedResponseAsync(request.Page, request.PageSize);
    }
}