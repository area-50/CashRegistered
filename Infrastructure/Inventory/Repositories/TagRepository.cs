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

public class TagRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : ITagRepository
{
    public async  Task CreateAsync(Tag entity)
    {
        await context.AddAsync(entity);
    }

    public async Task<Tag?> GetByIdAsync(int id)
    {
        return await context.Tags
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Tag>> FindAsync(Expression<Func<Tag, bool>> predicate)
    {
        return await context.Tags.Where(predicate).ToListAsync();
    }

    public void Update(Tag entity)
    {
        context.Tags.Update(entity);
    }

    public void Delete(Tag entity)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResponse<Tag>> SearchAsync(SearchTagRequest request)
    {
        var query = context.Tags.AsNoTracking().AsQueryable();
            
        if (string.IsNullOrWhiteSpace(request.Term))
            return await query.OrderByDescending(t => t.Id).ToPagedResponseAsync(request.Page, request.PageSize);

        var term = sqlUtils.SqlLikeContains(request.Term.ToLower());

        query = query.Where(t =>
            EF.Functions.ILike(t.Name, term) ||
            t.HexColor != null &&
            EF.Functions.ILike(t.HexColor, term)
        );
        return await query.OrderByDescending(t => t.Id).ToPagedResponseAsync(request.Page, request.PageSize);
    }
}