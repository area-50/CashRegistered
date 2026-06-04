using Domain.Inventory.Entities;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Response;

namespace Application.Inventory.Interfaces;

public interface IProductUseCase
{
    Task<CreateResponse> CreateProduct(CreateProductRequest createProductRequest);
    
    Task<PagedResponse<GetSearchProductResponse>> SearchProducts(SearchProductRequest searchProductRequest);
    
    Task<Product?> GetById(int productId);
    
    Task<GetProductByIdResponse> GetProductById(int productId);

    Task<UpdateResponse> UpdateProduct(int id, UpdateProductRequest request);
    
    Task Deactivate(int productId);
}