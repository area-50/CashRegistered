using Domain.Inventory.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.Inventory.Request;
using Shared.Request;

namespace Application.Inventory.Interfaces;

public interface IProductUseCase
{
    Task<CreateResponse> CreateProduct(CreateProductRequest createProductRequest);
}