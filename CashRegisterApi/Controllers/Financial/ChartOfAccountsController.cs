using Application.Financial.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domain.Shared.DTOs;
using Shared.Financial.Request;


namespace CashRegister.Controllers.Financial;

[ApiController]
[Route("api/chart-of-accounts")]
public class ChartOfAccountsController(IChartOfAccountsUseCase useCase) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> CreateChartOfAccounts([FromBody] CreateChartOfAccountsRequest request)
    {
        var response = await useCase.CreateChartOfAccounts(request);
        return Ok(response);
    }

    [HttpGet("search")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> SearchChartOfAccounts([FromQuery] SearchChartOfAccountsRequest request)
    {
        var response = await useCase.SearchChartOfAccounts(request);
        return Ok(response);
    }

    [HttpGet("{id}/GetChartOfAccountsById")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> GetChartOfAccountsById([FromRoute] int id)
    {
        var response = await useCase.GetChartOfAccountsById(id);
        return Ok(response);
    }

    [HttpPut("{id}/Update")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> UpdateChartOfAccounts(
        [FromRoute] int id, [FromBody] UpdateChartOfAccountsRequest request
    )
    {
        var response = await useCase.UpdateChartOfAccounts(id, request);
        return Ok(response);
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> DeactivateChartOfAccounts([FromRoute] int id)
    {
        await useCase.DeactivateChartOfAccounts(id);
        return Ok();
    }
}
