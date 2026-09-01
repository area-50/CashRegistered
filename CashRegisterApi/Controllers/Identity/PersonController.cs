using System.Threading.Tasks;
using Application.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domain.Shared.DTOs;

namespace CashRegisterApi.Controllers.Identity;

[Route("api/[controller]")]
[ApiController]
public class PersonController(IPersonUseCase person) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreatePerson(CreatePersonRequest request)
    {
        var response = await person.CreatePerson(request);
        return Created(string.Empty, response);
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetPeople()
    {
        var response = await person.GetAllPeople();
        return Ok(response);
    }

    [HttpGet("email")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetPersonByEmail([FromQuery] string email)
    {
        var result = await person.GetPersonByEmail(email);
        return Ok(result);
    }

    [HttpGet("taxid")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetPersonByTaxId([FromQuery] string taxId)
    {
        var result = await person.GetPersonByTaxId(taxId);
        return Ok(result);
    }
}
