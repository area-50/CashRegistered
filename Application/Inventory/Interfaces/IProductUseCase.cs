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
    
    Task<Shared.Inventory.Response.GetProductByIdResponse> GetProductById(int productId);
    Task<IEnumerable<Shared.Inventory.Response.ProductConversionItemResponse>> GetProductConversions(int productId);
    Task<Shared.Response.UpdateResponse> UpdateProduct(int id, Shared.Inventory.Request.UpdateProductRequest request);
    
    Task Deactivate(int productId);
}