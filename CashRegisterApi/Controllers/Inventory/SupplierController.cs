using Application.Inventory.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Inventory.Request;

namespace CashRegisterApi.Controllers.Inventory;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SupplierController(ISupplierUseCase supplierUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest request)
    {
        var response = await supplierUseCase.CreateSupplier(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSupplierById(int id)
    {
        var response = await supplierUseCase.GetSupplierById(id);
        if (response == null) return NotFound();
        return Ok(response);
    }

    [HttpGet("Search")]
    public async Task<IActionResult> SearchSuppliers([FromQuery] SearchSupplierRequest request)
    {
        var response = await supplierUseCase.SearchSuppliers(request);
        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierRequest request)
    {
        await supplierUseCase.UpdateSupplier(id, request);
        return Ok();
    }

    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateSupplier(int id)
    {
        await supplierUseCase.DeactivateSupplier(id);
        return Ok();
    }
}
