using System.Security.Claims;
using Application.Inventory.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Response;

namespace CashRegister.Controllers.Inventory;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ComercialOnly")] // Requisitions could be made by comercial, but fulfillment is by logistics/admin. Let's keep it broadly authenticated and specify permissions per action.
public class InventoryRequisitionsController(IInventoryRequisitionUseCase useCase) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<ActionResult<CreateResponse>> Create([FromBody] CreateInventoryRequisitionRequest request)
    {
        var userIdString = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                           ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

        request.RequestedByUserId = userId;
        var result = await useCase.CreateRequisitionAsync(request);
        if (result.Id == 0) return BadRequest();
        return Created($"/api/inventoryrequisitions/{result.Id}", result);
    }

    [HttpGet]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<ActionResult<PagedResponse<SearchInventoryRequisitionResponse>>> Search([FromQuery] SearchInventoryRequisitionRequest request)
    {
        var result = await useCase.SearchAsync(request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<ActionResult<GetInventoryRequisitionByIdResponse>> GetById(int id)
    {
        var result = await useCase.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id}/fulfill")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<ActionResult<UpdateResponse>> Fulfill(int id, [FromBody] FulfillInventoryRequisitionRequest request)
    {
        var userIdString = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                           ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

        var result = await useCase.FulfillRequisitionAsync(id, userId, request);
        if (result.Id == 0) return BadRequest();
        return Ok(result);
    }

    [HttpPut("{id}/cancel")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<ActionResult<UpdateResponse>> Cancel(int id)
    {
        var result = await useCase.CancelRequisitionAsync(id);
        if (result.Id == 0) return BadRequest();
        return Ok(result);
    }

    [HttpGet("pending/count")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<ActionResult<int>> GetPendingCount()
    {
        var count = await useCase.GetPendingCountAsync();
        return Ok(count);
    }
}
