using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Common;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Response;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Inventory.Repositories;

public class InventoryRequisitionRepository(CashRegisterDbContext context) : IInventoryRequisitionRepository
{
    public async Task CreateAsync(InventoryRequisition entity)
    {
        await context.InventoryRequisitions.AddAsync(entity);
    }

    public async Task<InventoryRequisition?> GetByIdAsync(int id)
    {
        return await context.InventoryRequisitions
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<InventoryRequisition>> FindAsync(Expression<Func<InventoryRequisition, bool>> predicate)
    {
        return await context.InventoryRequisitions.Where(predicate).ToListAsync();
    }

    public void Update(InventoryRequisition entity)
    {
        context.InventoryRequisitions.Update(entity);
    }

    public void Delete(InventoryRequisition entity)
    {
        context.InventoryRequisitions.Remove(entity);
    }

    public async Task<PagedResponse<SearchInventoryRequisitionResponse>> SearchAsync(SearchInventoryRequisitionRequest request)
    {
        var query = context.InventoryRequisitions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.OriginModule))
            query = query.Where(x => x.OriginModule == request.OriginModule);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<Domain.Inventory.Enums.RequisitionStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(x => x.Status == statusEnum);
            }
        }

        if (request.StartDate.HasValue)
            query = query.Where(x => x.CreatedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(x => x.CreatedAt <= request.EndDate.Value);

        var pagedRequisitions = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ToPagedResponseAsync(request.Page, request.PageSize);

        var dbItems = pagedRequisitions.Items;

        var userIds = dbItems.Select(x => x.RequestedByUserId).Distinct().ToList();
        var usersDict = await context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName);

        var items = dbItems.Select(x => new SearchInventoryRequisitionResponse
        {
            Id = x.Id,
            OriginModule = x.OriginModule,
            RequestedByUserId = x.RequestedByUserId,
            RequestedByUserName = usersDict.GetValueOrDefault(x.RequestedByUserId) ?? "Sistema",
            Status = x.Status.ToString(),
            CreatedAt = x.CreatedAt,
            FulfilledAt = x.FulfilledAt,
            IsActive = true
        }).ToList();

        return new PagedResponse<SearchInventoryRequisitionResponse>
        {
            Items = items,
            Page = pagedRequisitions.Page,
            PageSize = pagedRequisitions.PageSize,
            TotalCount = pagedRequisitions.TotalCount
        };
    }

    public async Task<GetInventoryRequisitionByIdResponse?> GetByIdResponseAsync(int id)
    {
        var req = await context.InventoryRequisitions
            .AsNoTracking()
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (req == null) return null;

        var userName = await context.Users.Where(u => u.Id == req.RequestedByUserId)
            .Select(u => u.UserName).FirstOrDefaultAsync() ?? "Sistema";

        return new GetInventoryRequisitionByIdResponse
        {
            Id = req.Id,
            OriginModule = req.OriginModule,
            RequestedByUserId = req.RequestedByUserId,
            RequestedByUserName = userName,
            Status = req.Status.ToString(),
            CreatedAt = req.CreatedAt,
            FulfilledAt = req.FulfilledAt,
            Notes = req.Notes,
            Items = req.Items.Select(i => new GetInventoryRequisitionItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Quantity = i.Quantity
            }).ToList()
        };
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await context.InventoryRequisitions.CountAsync(x => x.Status == Domain.Inventory.Enums.RequisitionStatus.Pending);
    }
}
