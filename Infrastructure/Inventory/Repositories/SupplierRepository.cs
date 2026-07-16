using System.Linq.Expressions;
using Domain.Inventory.Entities;
using Domain.Inventory.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using Shared.Response;

using Shared.Inventory.Request;

namespace Infrastructure.Inventory.Repositories;

public class SupplierRepository(CashRegisterDbContext context) : ISupplierRepository
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

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var search = request.Name.ToLower();
            query = query.Where(s => 
                s.Person.Name.FirstName.ToLower().Contains(search) || 
                s.Person.Name.LastName.ToLower().Contains(search)
            );
        }

        if (!string.IsNullOrWhiteSpace(request.TaxId))
        {
            query = query.Where(s => s.Person.TaxId != null && s.Person.TaxId.Contains(request.TaxId));
        }

        return await query
            .OrderByDescending(s => s.Id)
            .ToPagedResponseAsync(request.Page, request.PageSize);
    }
}
