using Application.Financial.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domain.Shared.DTOs;
using Shared.Financial.Request;

namespace CashRegister.Controllers.Financial;

[ApiController]
[Route("api/cost-centers")]
[Authorize]
public class CostCentersController(ICostCenterUseCase useCase) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateCostCenter([FromBody] CreateCostCenterRequest request)
    {
        var response = await useCase.CreateCostCenter(request);
        return Ok(response);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchCostCenters([FromQuery] SearchCostCenterRequest request)
    {
        var response = await useCase.SearchCostCenters(request);
        return Ok(response);
    }

    [HttpGet("{id}/GetCostCenterById")]
    public async Task<IActionResult> GetCostCenterById([FromRoute] int id)
    {
        var response = await useCase.GetCostCenterById(id);
        return Ok(response);
    }

    [HttpPut("{id}/Update")]
    public async Task<IActionResult> UpdateCostCenter(
        [FromRoute] int id, [FromBody] UpdateCostCenterRequest request
    )
    {
        var response = await useCase.UpdateCostCenter(id, request);
        return Ok(response);
    }

    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateCostCenter([FromRoute] int id)
    {
        await useCase.DeactivateCostCenter(id);
        return Ok();
    }
}
