using Application.Inventory.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Inventory.Request;

namespace CashRegister.Controllers.Inventory;

[Route("api/[controller]")]
[ApiController]
public class TagController(ITagUseCase tagUseCase) : ControllerBase
{
    [HttpPost]
    [Authorize (Policy = "LogisticsOnly")]
    public async Task<IActionResult> CreateTage([FromBody] CreateTagRequest request)
    {
        var response = await tagUseCase.CreateTag(request);
        return Ok(response);
    }

    [HttpGet("Search")]
    [Authorize (Policy = "LogisticsOnly")]
    public async Task<IActionResult> SearchTags([FromQuery] SearchTagRequest request)
    {
        var response = await tagUseCase.SearchTags(request);
        return Ok(response);
    }

    [HttpGet("{id}/GetTagById")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> GetTagById([FromRoute] int id)
    {
        var response = await tagUseCase.GetTagByIdResponse(id);
        return Ok(response);
    }

    [HttpPut("{id}/update")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> UpdateTag([FromRoute] int id, [FromBody] UpdateTagRequest request)
    {
        var response = await tagUseCase.UpdateTag(id, request);
        return Ok(response);
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> DeactivateTag([FromRoute] int id)
    {
        await tagUseCase.DeactivateTag(id);
        return NoContent();
    }
}