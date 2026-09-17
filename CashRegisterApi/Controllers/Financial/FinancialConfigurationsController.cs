using Application.Financial.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashRegister.Controllers.Financial;

[ApiController]
[Route("api/financial-configurations")]
public class FinancialConfigurationsController(IFinancialConfigurationUseCase useCase) : ControllerBase
{
    [HttpGet("GetFinancialConfiguration")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> GetFinancialConfiguration()
    {
        var response = await useCase.GetFinancialConfiguration();
        return Ok(response);
    }

    [HttpPut("Update")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateFinancialConfiguration([FromBody] UpdateFinancialConfigurationRequest request)
    {
        var response = await useCase.UpdateFinancialConfiguration(request);
        return Ok(response);
    }
}
