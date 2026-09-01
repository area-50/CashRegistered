using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;

using Domain.Shared.Abstractions;
using Domain.Shared.DTOs;
using Domain.Shared.DTOs;
using Domain.Shared.Notifications;
using Domain.Shared.Response;

namespace Application.Inventory.UseCases;

public class ProductUseCase(
    ITagUseCase tagUseCase,
    IProductRepository repository,
    IStockBalanceUseCase stockBalanceUseCase,
    IWarehouseUseCase warehouseUseCase,
    IUomConversionUseCase uomConversionUseCase,
    NotificationContext notificationContext,
    IUnitOfWork unitOfWork
) : IProductUseCase
{
    public async Task<CreateResponse> CreateProduct(CreateProductRequest createProductRequest)
    {
        var tagsSelected = await tagUseCase.GetTagByIds(createProductRequest.TagIds);

        var product = new Product(
            createProductRequest.Sku,
            createProductRequest.Name,
            createProductRequest.CategoryId,
            createProductRequest.BaseUomId,
            tagsSelected.ToList(),
            createProductRequest.Description,
            createProductRequest.NcmCode
        );

        if (product.IsInvalid)
        {
            notificationContext.AddNotifications(product.Notifications);
            return new CreateResponse {Id = 0};
        }
        
        await repository.CreateAsync(product);
        await unitOfWork.CommitAsync();

        if (product.IdIsZero())
        {
            notificationContext.AddNotification(
                "Produto",
                "Não foi possível criar o produto."
            );
            return new CreateResponse {Id = 0};
        }
        
        var warehouses = await warehouseUseCase.ListAll();
        var stockBalances = warehouses.Select(w => 
            new StockBalance(product.Id, w.Id)
        );
        
        await stockBalanceUseCase.AddRangeAsync(stockBalances);
        await unitOfWork.CommitAsync();
        
        return new CreateResponse {Id = product.Id};
    }


    public async Task<PagedResponse<GetSearchProductResponse>> SearchProducts(SearchProductRequest searchProductRequest)
    {
        var pagedProducts = await repository.SearchAsync(searchProductRequest);

        return new PagedResponse<GetSearchProductResponse>
        {
            Items = pagedProducts.Items.Select(p =>
            {
                var sb = p.StockBalances.FirstOrDefault();
                return new GetSearchProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Sku = p.Sku,
                    Category = p.Category.Name,
                    UomSymbol = p.BaseUom.Code,
                    BaseUomId = p.BaseUomId,
                    IsActive = p.IsActive,
                    StockQuantity = sb?.AvailableQuantity ?? 0,
                    WarehouseName = sb?.Warehouse.Name
                };
            }),
            Page = pagedProducts.Page,
            PageSize = pagedProducts.PageSize,
            TotalCount =  pagedProducts.TotalCount
        };
    }

    public async Task<Product?> GetById(int productId)
    {
        return await repository.GetByIdAsync(productId);
    }

    public async Task<GetProductByIdResponse> GetProductById(int productId)
    {
        var product = await repository.GetByIdAsync(productId);

        if (Product.NotExists(product, notificationContext)) return new GetProductByIdResponse();

        return new GetProductByIdResponse
        {
            Id = product!.Id,
            Name = product.Name,
            Sku = product.Sku,
            Description = product.Description,
            NcmCode = product.NcmCode,
            BaseUomId = product.BaseUomId,
            CategoryId = product.CategoryId,
            IsActive = product.IsActive,
            TagIds = product.Tags.Select(t => t.Id).ToList()
        };
    }

    public async Task<UpdateResponse> UpdateProduct(int id, UpdateProductRequest request)
    {
        var product = await repository.GetByIdAsync(id);

        if (Product.NotExists(product, notificationContext)) return new UpdateResponse {Id = 0};

        var tagsSelected = await tagUseCase.GetTagByIds(request.TagIds);

        product!.Update(
            request.Sku,
            request.Name,
            request.CategoryId,
            request.BaseUomId,
            tagsSelected.ToList(),
            request.IsActive,
            request.Description,
            request.NcmCode
        );
        
        repository.Update(product);
        await unitOfWork.CommitAsync();
        return new UpdateResponse { Id = product.Id };
    }

    public async Task Deactivate(int productId)
    {
        var product = await GetById(productId);

        if (Product.NotExists(product, notificationContext)) return;

        if (product is { IsActive: false })
        {
            notificationContext.AddNotification("Produto", "Produto já desativado");
            return;
        }

        product!.Deactivate();
    
        repository.Update(product);
        await unitOfWork.CommitAsync();
    }

    public async Task<IEnumerable<ProductConversionItemResponse>> GetProductConversions(int productId)
    {
        var product = await repository.GetByIdAsync(productId);
        if (product == null) return Enumerable.Empty<ProductConversionItemResponse>();

        var result = new List<ProductConversionItemResponse>
        {
            new()
            {
                UomId = product.BaseUomId,
                UomSymbol = product.BaseUom!.Code,
                Multiplier = 1,
                RuleType = "Base"
            }
        };

        var otherRules = await uomConversionUseCase.GetRulesForProductAsync(productId, product.BaseUomId);
        
        var uniqueOtherRules = otherRules
            .Where(r => r.UomId != product.BaseUomId)
            .OrderByDescending(r => r.RuleType == "ProductSpecific" ? 1 : 0);

        result.AddRange(uniqueOtherRules);

        return result;
    }
}