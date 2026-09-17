using Application.Financial.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domain.Shared.DTOs;
using Shared.Financial.Request;

namespace CashRegister.Controllers.Financial;

[ApiController]
[Route("api/cost-centers")]
public class CostCentersController(ICostCenterUseCase useCase) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> CreateCostCenter([FromBody] CreateCostCenterRequest request)
    {
        var response = await useCase.CreateCostCenter(request);
        return Ok(response);
    }

    [HttpGet("search")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> SearchCostCenters([FromQuery] SearchCostCenterRequest request)
    {
        var response = await useCase.SearchCostCenters(request);
        return Ok(response);
    }

    [HttpGet("{id}/GetCostCenterById")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> GetCostCenterById([FromRoute] int id)
    {
        var response = await useCase.GetCostCenterById(id);
        return Ok(response);
    }

    [HttpPut("{id}/Update")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> UpdateCostCenter(
        [FromRoute] int id, [FromBody] UpdateCostCenterRequest request
    )
    {
        var response = await useCase.UpdateCostCenter(id, request);
        return Ok(response);
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> DeactivateCostCenter([FromRoute] int id)
    {
        await useCase.DeactivateCostCenter(id);
        return Ok();
    }
}
