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

public class UnitOfMeasureRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : IUnitOfMeasureRepository
{
    public async Task<PagedResponse<UnitOfMeasure>> SearchAsync(SearchUnitOfMeasureRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Term))
            return await context.UnitsOfMeasure.OrderByDescending(u => u.Id).ToPagedResponseAsync(request.Page, request.PageSize);
        
        var term = sqlUtils.SqlLikeContains(request.Term.ToLower());

        return await context.UnitsOfMeasure
            .Where(u =>
                EF.Functions.ILike(u.Name, term) ||
                EF.Functions.ILike(u.Code, term)
            )
            .OrderByDescending(u => u.Id)
            .ToPagedResponseAsync(request.Page, request.PageSize);
    }

    public async Task CreateAsync(UnitOfMeasure entity)
    {
        await context.UnitsOfMeasure.AddAsync(entity);
    }

    public async Task<UnitOfMeasure?> GetByIdAsync(int id)
    {
        return await context.UnitsOfMeasure
            .Where(u => u.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UnitOfMeasure>> FindAsync(Expression<Func<UnitOfMeasure, bool>> predicate)
    {
        return await context.UnitsOfMeasure
            .Where(predicate)
            .ToListAsync();
    }

    public void Update(UnitOfMeasure entity)
    {
        context.UnitsOfMeasure.Update(entity);
    }

    public void Delete(UnitOfMeasure entity)
    {
        throw new NotImplementedException();
    }
}