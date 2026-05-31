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
    [Authorize(Policy = "LogisticsOnly")]
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

}