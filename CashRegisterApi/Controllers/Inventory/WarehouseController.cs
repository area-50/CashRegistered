using Application.Inventory.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Inventory.Request;

namespace CashRegister.Controllers.Inventory;

[ApiController]
[Route("api/warehouses")]
[Authorize(Policy = "LogisticsOnly")]
public class WarehouseController(IWarehouseUseCase useCase) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseRequest request)
    {
        var result = await useCase.CreateWarehouse(request);
        return Ok(result); 
    }

    [HttpGet("search")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> SearchWarehouses([FromQuery] SearchWarehouseRequest request)
    {
        return Ok(await useCase.SearchWarehouses(request));
    }

    [HttpGet("{id}/GetWarehouseById")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> GetWarehouseById(int id)
    {
        return Ok(await useCase.GetWarehouseById(id));
    }

    [HttpPut("{id}/UpdateWarehouse")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] UpdateWarehouseRequest request)
    {
        return Ok(await useCase.UpdateWarehouse(id, request));
    }

    [HttpPut("{id}/DeactivateWarehouse")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> DeactivateWarehouse(int id)
    {
        await useCase.DeactivateWarehouse(id);
        return Ok();
    }
}
