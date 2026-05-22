using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;

using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Notifications;
using Shared.Request;

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
}