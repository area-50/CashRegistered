using Application.Inventory.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Inventory.Request;

namespace CashRegisterApi.Controllers.Inventory;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryTransactionController(IInventoryTransactionUseCase inventoryTransactionUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryTransactionRequest request)
    {
        var response = await inventoryTransactionUseCase.CreateTransaction(request);

        if (response.Id == 0)
        {
            return BadRequest();
        }

        return Ok(response);
    }
}
