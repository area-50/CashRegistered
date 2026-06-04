using Application.Inventory.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Inventory.Request;

namespace CashRegister.Controllers.Inventory;

[Route("api/[controller]")]
[ApiController]
public class UomConversionController(
    IUomConversionUseCase uomConversionCase    
) : ControllerBase
{
    [HttpPost]
    [Authorize (Policy = "LogisticsOnly")]
    public async Task<IActionResult> CreateUomConversion(
        [FromBody] CreateUomConversionRequest request
    )
    {
        var response = await uomConversionCase.CreateUomConversion(request);
        return Ok(response);
    }

    [HttpGet("Search")]
    [Authorize (Policy = "LogisticsOnly")]
    public async Task<IActionResult> SearchUomConversion(
        [FromQuery] SearchUomConversionRequest request
    )
    {
        var response = await uomConversionCase.SearchUomConversion(request);
        return Ok(response);
    }

    [HttpPut("{id}/Deactivate")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> DeactivateUomConversion([FromRoute] int id)
    {
        await   uomConversionCase.DeactivateUomConversion(id);
        return NoContent();
    }

    [HttpGet("{id}/GetConversionById")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> GetConversionById([FromRoute] int id)
    {
        var response = await uomConversionCase.GetUomConversionById(id);
        return Ok(response);
    }

    [HttpPut("{id}/Update")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> UpdateUomConversion([FromRoute] int id, [FromBody] UpdateUomConversionRequest request)
    {
        var response = await uomConversionCase.UpdateUomConversion(id, request);
        return Ok(response);
    }
}