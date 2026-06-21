using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Identity.Request;
using Shared.Security.Request;

namespace CashRegister.Controllers.Identity;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserUseCase user) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserPayload payload
    )
    {
        var response = await user.CreateUser(payload.UserRequest, payload.PersonRequest);
        return Created(string.Empty, response);
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetUsers()
    {
        var result = await user.GetAllUsers();
        return Ok(result);
    }

    [HttpGet("Search")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Search([FromQuery] SearchUserRequest request)
    {
        var result = await user.SearchUsers(request);
        return Ok(result);
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        await user.DeactivateUser(id);
        return Ok();
    }

    [HttpPut("ChangePassword")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                           ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!int.TryParse(userIdString, out int userId)) return Unauthorized();
        
        await user.ChangePassword(userId, request);
        return Ok();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                           ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!int.TryParse(userIdString, out int userId)) return Unauthorized();
        
        var response = await user.GetMe(userId);
        
        if (string.IsNullOrEmpty(response.UserName)) return NotFound();

        return Ok(new Shared.Response.ApiResponse<object>
        {
            Data = response
        });
    }
}

