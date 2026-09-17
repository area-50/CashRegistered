using System.Linq.Expressions;
using Domain.Identity.Entities;
using Domain.Identity.Repositories;
using Domain.Shared.DTOs;
using Domain.Shared.Response;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Infrastructure.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity.Repositories;

public class SupplierRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : ISupplierRepository
{
    public async Task CreateAsync(Supplier entity)
    {
        await context.Suppliers.AddAsync(entity);
    }

    public async Task<Supplier?> GetByIdAsync(int id)
    {
        return await context.Suppliers
            .Include(s => s.Person)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Supplier>> FindAsync(Expression<Func<Supplier, bool>> predicate)
    {
        return await context.Suppliers
            .Include(s => s.Person)
            .Where(predicate)
            .ToListAsync();
    }

    public void Update(Supplier entity)
    {
        context.Suppliers.Update(entity);
    }

    public void Delete(Supplier entity)
    {
        context.Suppliers.Remove(entity);
    }

    public async Task<PagedResponse<Supplier>> SearchAsync(SearchSupplierRequest request)
    {
        var query = context.Suppliers
            .Include(s => s.Person)
            .AsNoTracking();

        query = sqlUtils.WhereAnd(
            query, request.IsActive.HasValue, s => s.IsActive == request.IsActive!.Value
        );
        query = sqlUtils.WhereAnd(
            query, !string.IsNullOrWhiteSpace(request.Name),
            s => EF.Functions.ILike(
                s.Person.Name.FirstName, sqlUtils.SqlLikeContains(request.Name!.Trim())
            ) ||
            EF.Functions.ILike(
                s.Person.Name.LastName, sqlUtils.SqlLikeContains(request.Name!.Trim())
            ) ||
            (s.Person.TradeName != null && EF.Functions.ILike(
                s.Person.TradeName, sqlUtils.SqlLikeContains(request.Name!.Trim())
            ))
        );
        query = sqlUtils.WhereAnd(
            query, !string.IsNullOrWhiteSpace(request.TaxId),
            s => EF.Functions.ILike(
                s.Person.TaxId, sqlUtils.SqlLikeContains(request.TaxId!.Trim())
            )
        );

        return await query
            .OrderByDescending(s => s.CreatedAt)
            .ToPagedResponseAsync(request.Page, request.PageSize);
    }
}
