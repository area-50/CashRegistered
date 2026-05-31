using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;

using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Notifications;
using Shared.Request;
using Shared.Response;

namespace Application.Inventory.UseCases;

public class ProductUseCase(
    ITagUseCase tagUseCase,
    IProductRepository repository,
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
        
        return new CreateResponse {Id = product.Id};
    }

    public async Task<PagedResponse<GetSearchProductResponse>> SearchProducts(SearchProductRequest searchProductRequest)
    {
        var pagedProducts = await repository.SearchAsync(searchProductRequest);

        return new PagedResponse<GetSearchProductResponse>
        {
            Items = pagedProducts.Items.Select(p =>
                new GetSearchProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Category = p.Category.Name,
                UomSymbol = p.BaseUom.Code,
                IsActive = p.IsActive
            }
            ),
            Page = pagedProducts.Page,
            PageSize = pagedProducts.PageSize,
            TotalCount =  pagedProducts.TotalCount
        };
    }

    public async Task<Product?> GetProductById(int productId)
    {
        return await repository.GetByIdAsync(productId);
    }

    public async Task Deactivate(int productId)
    {
        var product = await GetProductById(productId);

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
}