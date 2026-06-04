using Application.Inventory.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Inventory.Request;

namespace CashRegister.Controllers.Inventory;

[Route("api/[controller]")]
[ApiController]
public class UnitOfMeasureController(
    IUnitOfMeasureUseCase unitOfMeasureUseCase
) : ControllerBase
{
    [HttpPost]
    [Authorize (Policy = "LogisticsOnly")]
    public async Task<IActionResult> CreateUnitOfMeasure(CreateUnitOfMeasureRequest request)
    {
        var response = await unitOfMeasureUseCase.CreateUnitOfMeasure(request);
        return Ok(response);
    }

    [HttpGet("search")]
    [Authorize (Policy = "LogisticsOnly")]
    public async Task<IActionResult> SearchUnits([FromQuery] SearchUnitOfMeasureRequest request)
    {
        var response = await unitOfMeasureUseCase.SearchUnits(request);
        return Ok(response);
    }

    [HttpGet("{id}/GetUnitById")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> GetUnitById([FromRoute] int id)
    {
        var response = await unitOfMeasureUseCase.GetUnitOfMeasureById(id);
        return Ok(response);
    }

    [HttpPut("{id}/update")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> UpdateUnitOfMeasure([FromRoute] int id, [FromBody] UpdateUnitOfMeasureRequest request)
    {
        var response = await unitOfMeasureUseCase.UpdateUnitOfMeasure(id, request);
        return Ok(response);
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> DeactivateUnitOfMeasure([FromRoute] int id)
    {
        await unitOfMeasureUseCase.DeactivateUnitOfMeasure(id);
        return NoContent();
    }
}