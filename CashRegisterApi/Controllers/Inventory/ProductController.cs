using Application.Inventory.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Inventory.Request;

namespace CashRegister.Controllers.Inventory;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IProductUseCase productUseCase) : ControllerBase
{
    [HttpPost]
    [Authorize (Policy = "LogisticsOnly")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest createProductRequest)
    {
        var response = await productUseCase.CreateProduct(createProductRequest);
        return Ok(response);
    }

    [HttpGet("Search")]
    [Authorize]
    public async Task<IActionResult> SearchProducts([FromQuery] SearchProductRequest searchProductRequest)
    {
        var response = await productUseCase.SearchProducts(searchProductRequest);
        return Ok(response);
    }

    [HttpPut("{productId}/Deactivate")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> DeactivateProduct(int productId)
    {
        await productUseCase.Deactivate(productId);
        return Ok();
    }

    [HttpGet("{id}/GetProductById")]
    [Authorize]
    public async Task<IActionResult> GetProductById([FromRoute] int id)
    {
        var response = await productUseCase.GetProductById(id);
        return Ok(response);
    }

    [HttpPut("{id}/Update")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] UpdateProductRequest request)
    {
        var response = await productUseCase.UpdateProduct(id, request);
        return Ok(response);
    }
    }