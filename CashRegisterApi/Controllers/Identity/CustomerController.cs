using Application.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashRegister.Controllers.Identity;

[ApiController]
[Route("api/[controller]")]
public class CustomerController(ICustomerUseCase customerUseCase) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "CommercialOnly")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
    {
        var response = await customerUseCase.CreateCustomer(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "CommercialOnly")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await customerUseCase.GetCustomerById(id);
        return Ok(response);
    }

    [HttpGet("Search")]
    [Authorize]
    public async Task<IActionResult> Search([FromQuery] Shared.Identity.Request.SearchCustomerRequest request)
    {
        var response = await customerUseCase.SearchCustomers(request);
        return Ok(response);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CommercialOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerRequest request)
    {
        await customerUseCase.UpdateCustomer(id, request);
        return NoContent();
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Policy = "CommercialOnly")]
    public async Task<IActionResult> DeactivateCustomer(int id)
    {
        await customerUseCase.DeactivateCustomer(id);
        return NoContent();
    }
}
