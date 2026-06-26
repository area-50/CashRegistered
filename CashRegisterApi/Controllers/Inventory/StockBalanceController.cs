using Application.Inventory.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Inventory.Request;

namespace CashRegisterApi.Controllers.Inventory;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockBalanceController(IStockBalanceUseCase useCase) : ControllerBase
{
    [HttpGet("Search")]
    public async Task<IActionResult> Search([FromQuery] SearchStockBalanceRequest request)
    {
        var response = await useCase.SearchAsync(request);
        return Ok(response);
    }

    [HttpGet("GetAvailableBalance")]
    public async Task<IActionResult> GetAvailableBalance([FromQuery] int productId, [FromQuery] int? warehouseId)
    {
        var balance = await useCase.GetAvailableBalanceAsync(productId, warehouseId);
        return Ok(balance);
    }
}
