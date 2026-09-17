using Application.Financial.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashRegister.Controllers.Financial;

[ApiController]
[Route("api/financial-accounts")]
public class FinancialAccountsController(IFinancialAccountUseCase useCase) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> CreateFinancialAccount([FromBody] CreateFinancialAccountRequest request)
    {
        var response = await useCase.CreateFinancialAccount(request);
        return Ok(response);
    }

    [HttpGet("search")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> SearchFinancialAccounts([FromQuery] SearchFinancialAccountRequest request)
    {
        var response = await useCase.SearchFinancialAccounts(request);
        return Ok(response);
    }

    [HttpGet("{id}/GetFinancialAccountById")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> GetFinancialAccountById([FromRoute] int id)
    {
        var response = await useCase.GetFinancialAccountById(id);
        return Ok(response);
    }

    [HttpPut("{id}/Update")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> UpdateFinancialAccount(
        [FromRoute] int id, [FromBody] UpdateFinancialAccountRequest request
    )
    {
        var response = await useCase.UpdateFinancialAccount(id, request);
        return Ok(response);
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> DeactivateFinancialAccount([FromRoute] int id)
    {
        await useCase.DeactivateFinancialAccount(id);
        return Ok();
    }
}
