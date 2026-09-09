using System.Linq.Expressions;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Domain.Shared.Response;
using Domain.Shared.DTOs;
using Infrastructure.Common;

namespace Infrastructure.Inventory.Repositories;

public class InventoryTransactionRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : IInventoryTransactionRepository
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

        query = sqlUtils.WhereLike(
            query, !string.IsNullOrWhiteSpace(request.ReferenceDocument), request.ReferenceDocument,
            x => x.ReferenceDocument
        );

        query = sqlUtils.WhereLike(
            query, !string.IsNullOrWhiteSpace(request.Name), request.Name,
            x => x.Name
        );

        query = sqlUtils.WhereLike(
            query, !string.IsNullOrWhiteSpace(request.Description), request.Description,
            x => x.Description
        );

        query = sqlUtils.WhereAnd(
            query, request.StartDate.HasValue,
            x => x.DateTime >= request.StartDate!.Value.ToUniversalTime()
        );

        query = sqlUtils.WhereAnd(
            query, request.EndDate.HasValue,
            x => x.DateTime <= request.EndDate!.Value.ToUniversalTime()
        );

        var isTypeValid = !string.IsNullOrWhiteSpace(request.TransactionType) &&
            Enum.TryParse<Domain.Inventory.Enums.TransactionType>(request.TransactionType, true, out var type);
        query = sqlUtils.WhereAnd(
            query, isTypeValid,
            x => x.Type == Enum.Parse<Domain.Inventory.Enums.TransactionType>(request.TransactionType!, true)
        );

        var isStatusValid = !string.IsNullOrWhiteSpace(request.TransactionStatus) &&
            Enum.TryParse<Domain.Inventory.Enums.TransactionStatus>(request.TransactionStatus, true, out var status);
        query = sqlUtils.WhereAnd(
            query, isStatusValid,
            x => x.Status == Enum.Parse<Domain.Inventory.Enums.TransactionStatus>(request.TransactionStatus!, true)
        );

        return await query
            .OrderByDescending(x => x.DateTime)
            .ThenByDescending(x => x.Id)
            .ToPagedResponseAsync(request.Page, request.PageSize);
    }

    public async Task<GetInventoryTransactionByIdResponse?> GetDetailsAsync(int id)
    {
        return await context.InventoryTransactions
            .Where(x => x.Id == id)
            .Select(x => new GetInventoryTransactionByIdResponse
            {
                Id = x.Id,
                TransactionType = x.Type.ToString(),
                ReferenceDocument = x.ReferenceDocument,
                Name = x.Name,
                Description = x.Description,
                CreatedAt = x.DateTime,
                TransactionStatus = x.Status.ToString(),
                Items = x.Items.Select(i => new InventoryTransactionItemResponse
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
