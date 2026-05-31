using Domain.Inventory.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Request;
using Shared.Response;

namespace Application.Inventory.Interfaces;

public interface IProductUseCase
{
    Task<CreateResponse> CreateProduct(CreateProductRequest createProductRequest);
    
    Task<PagedResponse<GetSearchProductResponse>> SearchProducts(SearchProductRequest searchProductRequest);
    
    Task<Product?> GetProductById(int productId);
    
    Task Deactivate(int productId);
}