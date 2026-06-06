using Application.Inventory.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Inventory.Request;

namespace CashRegister.Controllers.Inventory;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryUseCase categoryUseCase) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var response = await categoryUseCase.CreateCategory(request);
        return Ok(response);
    }
    
    [HttpGet("Search")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> GetSearchCategory([FromQuery] SearchCategoryRequest request)
    {
        var response = await categoryUseCase.GetSearchCategories(request);
        return Ok(response);
    }

    [HttpGet("{id}/GetCategoryById")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> GetCategoryById([FromRoute] int id)
    {
        var response = await categoryUseCase.GetCategoryByIdResponse(id);
        return Ok(response);
    }

    [HttpPut("{id}/Update")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> UpdateCategory([FromRoute] int id, [FromBody] UpdateCategoryRequest request)
    {
        var response = await categoryUseCase.UpdateCategory(id, request);
        return Ok(response);
    }

    [HttpPut("{id}/Deactivate")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task<IActionResult> DeactivateCategory([FromRoute] int id)
    {
        await categoryUseCase.DeactivateCategory(id);
        return Ok();
    }
}
