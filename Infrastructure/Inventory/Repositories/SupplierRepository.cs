using System.Linq.Expressions;
using Domain.Inventory.Entities;
using Domain.Inventory.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Common;
using Infrastructure.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Domain.Shared.Abstractions;
using Domain.Shared.Response;
using Domain.Shared.DTOs;

namespace Infrastructure.Inventory.Repositories;

public class SupplierRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : ISupplierRepository
{
    public async Task CreateAsync(Supplier entity) => await context.Suppliers.AddAsync(entity);

    public async Task<Supplier?> GetByIdAsync(int id) => await context.Suppliers
        .Include(s => s.Person)
        .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<IEnumerable<Supplier>> FindAsync(Expression<Func<Supplier, bool>> predicate) =>
        await context.Suppliers
            .Include(s => s.Person)
            .Where(predicate)
            .ToListAsync();

    public void Update(Supplier entity) => context.Suppliers.Update(entity);

    public void Delete(Supplier entity) => context.Suppliers.Remove(entity);

    public async Task<PagedResponse<Supplier>> SearchAsync(SearchSupplierRequest request)
    {
        var query = context.Suppliers
            .Include(s => s.Person)
            .AsNoTracking();

        query = sqlUtils.WhereAnd(
            query, !string.IsNullOrWhiteSpace(request.Name),
            s => EF.Functions.ILike(s.Person.Name.FirstName, sqlUtils.SqlLikeContains(request.Name!.Trim())) ||
                 EF.Functions.ILike(s.Person.Name.LastName, sqlUtils.SqlLikeContains(request.Name!.Trim()))
        );

        query = sqlUtils.WhereAnd(
            query, !string.IsNullOrWhiteSpace(request.TaxId),
            s => s.Person.TaxId != null && EF.Functions.ILike(s.Person.TaxId, sqlUtils.SqlLikeContains(request.TaxId!.Trim()))
        );

        return await query
            .OrderByDescending(s => s.Id)
            .ToPagedResponseAsync(request.Page, request.PageSize);
    }
}

