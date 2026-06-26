using System.Threading.Tasks;
using Application.Financial.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Financial.Request;

namespace CashRegister.Controllers.Financial;

[Obsolete("Funcionalidade depreciada.")]
[Route("api/[controller]")]
[ApiController]
public class CashFlowController(ICashFlowUseCase cashFlow) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "ComercialOnly")]
    public async Task<IActionResult> CreateCashFlow([FromBody] CreateCashFlowRequest request)
    {
        await cashFlow.CreateCashFlow(request);
        return Created();
    }

    [HttpGet("CashFlowAvailable")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> GetCashFlowsAvailable()
    {
        var response = await cashFlow.GetCashFlowsAvailable();
        return Ok(response);
    }

    [HttpGet("GetExpensesByCashFlowIdId/")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> GetExpensesByCashFlowId([FromQuery] int cashFlowId)
    {
        var response = await cashFlow.GetExpensesByCashFlowId(cashFlowId);
        return Ok(response);
    }

    [HttpPut("AddCash")]
    [Authorize(Policy = "ComercialOnly")]
    public async Task<IActionResult> AddCash([FromBody] AddCashRequest request)
    {
        await cashFlow.AddCash(request);
        return Ok();
    }
}
