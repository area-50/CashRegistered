using System.Security.Claims;
using Application.Inventory.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Inventory.Request;

namespace CashRegister.Controllers.Inventory;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryTransactionController(IInventoryTransactionUseCase inventoryTransactionUseCase) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> Create([FromBody] CreateInventoryTransactionRequest request)
    {
        var userIdString = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                           ?? User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        
        if (!int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(new { message = "Não foi possível identificar o usuário autenticado." });
        }
        
        request.UserId = userId;

        var response = await inventoryTransactionUseCase.CreateTransaction(request);

        if (response.Id == 0)
        {
            return BadRequest();
        }

        return Ok(response);
    }

    [HttpGet("Search")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> Search([FromQuery] SearchInventoryTransactionRequest request)
    {
        var response = await inventoryTransactionUseCase.SearchAsync(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await inventoryTransactionUseCase.GetByIdAsync(id);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }
}
