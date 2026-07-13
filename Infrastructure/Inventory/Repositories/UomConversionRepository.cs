using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Infrastructure.Utils.Interfaces;
using Shared.Inventory.Request;
using Shared.Response;

namespace Infrastructure.Inventory.Repositories;

public class UomConversionRepository(
    CashRegisterDbContext context,
    ISqlUtils sqlUtils
) : IUomConversionRepository
{
    public async Task CreateAsync(UomConversion entity)
    {
        await context.UomConversions.AddAsync(entity);
    }

    public Task<UomConversion?> GetByIdAsync(int id)
    {
        return context.UomConversions
            .Include(uom => uom.FromUom)
            .Include(uom => uom.ToUom)
            .Include(uom => uom.Product)
            .Where(uom => uom.Id == id)
            .FirstOrDefaultAsync();
    }

    public Task<IEnumerable<UomConversion>> FindAsync(Expression<Func<UomConversion, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public void Update(UomConversion entity)
    {
        context.UomConversions.Update(entity);
    }

    public void Delete(UomConversion entity)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResponse<UomConversion>> SearchAsync(SearchUomConversionRequest request)
    {
        var query = context.UomConversions
            .Include(u => u.FromUom)
            .Include(u => u.ToUom)
            .Include(u => u.Product)
            .AsQueryable();

        if (string.IsNullOrWhiteSpace(request.Term))
            return await query.OrderByDescending(u => u.Id)
                .ToPagedResponseAsync(request.Page, request.PageSize);
        
        var term = sqlUtils.SqlLikeContains(request.Term.ToLower());
        
        query = query.Where(uom =>
            string.IsNullOrWhiteSpace(term) ||
            uom.Multiplier.ToString().Contains(request.Term) ||
            EF.Functions.ILike(uom.FromUom.Name, term) ||
            EF.Functions.ILike(uom.FromUom.Code, term) ||
            EF.Functions.ILike(uom.ToUom.Name, term) ||
            EF.Functions.ILike(uom.ToUom.Code, term) ||
            uom.Product != null &&
            EF.Functions.ILike(uom.Product.Name, term) ||
            uom.Product != null &&
            EF.Functions.ILike(uom.Product.Sku, term)
        );

        return await query.OrderByDescending(u => u.Id).ToPagedResponseAsync(request.Page, request.PageSize);
    }
}