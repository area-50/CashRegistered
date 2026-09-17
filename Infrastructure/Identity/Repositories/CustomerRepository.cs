using System.Linq.Expressions;
using Domain.Identity.Entities;
using Domain.Identity.Repositories;
using Domain.Shared.Abstractions;
using Domain.Shared.DTOs;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Infrastructure.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity.Repositories;

public class CustomerRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : ICustomerRepository
{
    public async Task CreateAsync(Customer entity)
    {
        await context.Customers.AddAsync(entity);
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await context.Customers
            .Include(c => c.Person)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetByPersonIdAsync(int personId)
    {
        return await context.Customers
            .Include(c => c.Person)
            .FirstOrDefaultAsync(c => c.PersonId == personId);
    }

    public async Task<IEnumerable<Customer>> FindAsync(Expression<Func<Customer, bool>> predicate)
    {
        return await context.Customers
            .Include(c => c.Person)
            .Where(predicate)
            .ToListAsync();
    }

    public void Update(Customer entity)
    {
        context.Customers.Update(entity);
    }

    public void Delete(Customer entity)
    {
        context.Customers.Remove(entity);
    }

    public async Task<PagedResponse<Customer>> SearchAsync(Domain.Shared.DTOs.SearchCustomerRequest request)
    {
        var query = context.Customers
            .Include(c => c.Person)
            .AsNoTracking();

        query = sqlUtils.WhereAnd(
            query, request.IsActive.HasValue, c => c.IsActive == request.IsActive!.Value
        );
        query = sqlUtils.WhereAnd(
            query, !string.IsNullOrWhiteSpace(request.Name),
            c => EF.Functions.ILike(
                c.Person.Name.FirstName, sqlUtils.SqlLikeContains(request.Name!.Trim())
            ) ||
            EF.Functions.ILike(
                c.Person.Name.LastName, sqlUtils.SqlLikeContains(request.Name!.Trim())
            ) ||
            (c.Person.TradeName != null && EF.Functions.ILike(
                c.Person.TradeName, sqlUtils.SqlLikeContains(request.Name!.Trim())
            ))
        );
        query = sqlUtils.WhereAnd(
            query, !string.IsNullOrWhiteSpace(request.TaxId),
            c => EF.Functions.ILike(
                c.Person.TaxId, sqlUtils.SqlLikeContains(request.TaxId!.Trim())
            )
        );

        return await query
            .OrderByDescending(c => c.Id)
            .ToPagedResponseAsync(request.Page, request.PageSize);
    }
}
