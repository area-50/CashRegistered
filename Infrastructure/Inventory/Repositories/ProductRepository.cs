using System.Linq.Expressions;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Inventory.Repositories;

public class ProductRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : IProductRepository
{
    public async Task CreateAsync(Product entity)
    {
        await context.Products.AddAsync(entity);
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Product>> FindAsync(Expression<Func<Product, bool>> predicate)
    {
        return await context.Products.Where(predicate).ToListAsync();
    }

    public void Update(Product entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(Product entity)
    {
        throw new NotImplementedException();
    }
}