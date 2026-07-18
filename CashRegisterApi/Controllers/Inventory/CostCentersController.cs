using Application.Inventory.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Inventory.Request;

namespace CashRegisterApi.Controllers.Inventory;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "LogisticsOnly")]
public class CostCentersController(ICostCenterUseCase useCase) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateCostCenterRequest request)
    {
        var response = await useCase.CreateCostCenter(request);
        if (response.Id == 0) return BadRequest();
        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCostCenterRequest request)
    {
        await useCase.UpdateCostCenter(id, request);
        return NoContent();
    }

    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateCostCenter(int id)
    {
        await useCase.DeactivateCostCenter(id);
        return NoContent();
    }

    [HttpGet("{id}/GetCostCenterById")]
    public async Task<IActionResult> GetCostCenterById(int id)
    {
        var response = await useCase.GetCostCenterById(id);
        if (response == null) return NotFound();
        return Ok(response);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchCostCenterRequest request)
    {
        var response = await useCase.SearchCostCenters(request);
        return Ok(response);
    }
}
