using System.Linq.Expressions;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Infrastructure.Utils;
using Infrastructure.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Inventory.Request;
using Shared.Response;

namespace Infrastructure.Inventory.Repositories;

public class CategoryRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : ICategoryRepository
{
    public async Task CreateAsync(Category entity)
    {
        await context.Categories.AddAsync(entity);
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await context.Categories
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();
    }

    public Task<IEnumerable<Category>> FindAsync(Expression<Func<Category, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public void Update(Category entity)
    {
        context.Categories.Update(entity);
    }

    public void Delete(Category entity)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResponse<Category>> SearchAsync(SearchCategoryRequest request)
    {
        var query = context.Categories
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .AsQueryable();
        
        if (string.IsNullOrWhiteSpace(request.Term))
            return await query.OrderByDescending(c => c.Id).ToPagedResponseAsync(request.Page, request.PageSize);

        var term = sqlUtils.SqlLikeContains(request.Term.ToLower());

        query = query.Where(c =>
            EF.Functions.ILike(c.Name, term) ||
            (c.ParentCategory != null &&
             EF.Functions.ILike(c.ParentCategory.Name, term))
        );
        
        return await query.OrderByDescending(c => c.Id).ToPagedResponseAsync(request.Page, request.PageSize);
    }
}