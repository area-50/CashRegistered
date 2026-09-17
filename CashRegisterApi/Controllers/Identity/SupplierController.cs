using Application.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashRegister.Controllers.Identity;

[ApiController]
[Route("api/[controller]")]
public class SupplierController(ISupplierUseCase supplierUseCase) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request)
    {
        var response = await supplierUseCase.CreateSupplier(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await supplierUseCase.GetSupplierById(id);
        return Ok(response);
    }

    [HttpGet("Search")]
    [Authorize]
    public async Task<IActionResult> Search([FromQuery] SearchSupplierRequest request)
    {
        var response = await supplierUseCase.SearchSuppliers(request);
        return Ok(response);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierRequest request)
    {
        await supplierUseCase.UpdateSupplier(id, request);
        return NoContent();
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await supplierUseCase.DeactivateSupplier(id);
        return NoContent();
    }
}
