using System.Linq.Expressions;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Response;
using Shared.Inventory.Request;
using Infrastructure.Common;

namespace Infrastructure.Inventory.Repositories;

public class InventoryTransactionRepository(CashRegisterDbContext context) : IInventoryTransactionRepository
{
    public async Task CreateAsync(InventoryTransaction entity)
    {
        await context.InventoryTransactions.AddAsync(entity);
    }

    public async Task<InventoryTransaction?> GetByIdAsync(int id)
    {
        return await context.InventoryTransactions
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .Include(x => x.Items)
                .ThenInclude(i => i.SourceWarehouse)
            .Include(x => x.Items)
                .ThenInclude(i => i.DestinationWarehouse)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<InventoryTransaction>> FindAsync(Expression<Func<InventoryTransaction, bool>> predicate)
    {
        return await context.InventoryTransactions
            .Include(x => x.Items)
            .Where(predicate)
            .ToListAsync();
    }

    public void Update(InventoryTransaction entity)
    {
        context.InventoryTransactions.Update(entity);
    }

    public void Delete(InventoryTransaction entity)
    {
        context.InventoryTransactions.Remove(entity);
    }

    public async Task<PagedResponse<InventoryTransaction>> SearchAsync(SearchInventoryTransactionRequest request)
    {
        var query = context.InventoryTransactions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.ReferenceDocument))
        {
            var term = request.ReferenceDocument.ToLower();
            query = query.Where(x => x.ReferenceDocument != null && x.ReferenceDocument.ToLower().Contains(term));
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.DateTime >= request.StartDate.Value.ToUniversalTime());
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.DateTime <= request.EndDate.Value.ToUniversalTime());
        }

        if (!string.IsNullOrWhiteSpace(request.TransactionType) &&
            Enum.TryParse<Domain.Inventory.Enums.TransactionType>(
                request.TransactionType, true, out var type
            ))
        {
            query = query.Where(x => x.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(request.TransactionStatus) &&
            Enum.TryParse<Domain.Inventory.Enums.TransactionStatus>(
                request.TransactionStatus, true, out var status
            ))
        {
            query = query.Where(x => x.Status == status);
        }

        return await query
            .OrderByDescending(x => x.DateTime)
            .ThenByDescending(x => x.Id)
            .ToPagedResponseAsync(request.Page, request.PageSize);
    }

    public async Task<Shared.Inventory.Response.GetInventoryTransactionByIdResponse?> GetDetailsAsync(int id)
    {
        return await context.InventoryTransactions
            .Where(x => x.Id == id)
            .Select(x => new Shared.Inventory.Response.GetInventoryTransactionByIdResponse
            {
                Id = x.Id,
                TransactionType = x.Type.ToString(),
                ReferenceDocument = x.ReferenceDocument,
                Name = x.Name,
                Description = x.Description,
                CreatedAt = x.DateTime,
                TransactionStatus = x.Status.ToString(),
                Items = x.Items.Select(i => new Shared.Inventory.Response.InventoryTransactionItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Quantity = i.TransactionQuantity,
                    SourceWarehouseId = i.SourceWarehouseId,
                    SourceWarehouseName = i.SourceWarehouse != null ? i.SourceWarehouse.Name : null,
                    DestinationWarehouseId = i.DestinationWarehouseId,
                    DestinationWarehouseName = i.DestinationWarehouse != null ? i.DestinationWarehouse.Name : null,
                    UomSymbol = i.Uom.Code,
                    UomName = i.Uom.Name
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }
}
