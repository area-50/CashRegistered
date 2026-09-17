using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashRegister.Controllers.Identity;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var result = await user.GetUserByIdResponse(id);
        if (result == null) return NotFound();
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

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateUserProfile([FromBody] Shared.Identity.Request.UpdateUserProfileRequest request)
    {
        var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                           ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

        await user.UpdateUserProfile(userId, request);
        return Ok();
    }

    [HttpPut("{id}/admin-update")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AdminUpdateUser([FromRoute] int id, [FromBody] AdminUpdateUserRequest request)
    {
        await user.AdminUpdateUser(id, request);
        return Ok();
    }

    [HttpPut("{id}/admin-reset-password")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AdminResetPassword([FromRoute] int id, [FromBody] Shared.Identity.Request.AdminResetPasswordRequest request)
    {
        await user.AdminResetPassword(id, request);
        return Ok();
    }

    [HttpPut("timezone")]
    [Authorize]
    public async Task<IActionResult> UpdateTimezone([FromBody] UpdateTimezoneRequest request)
    {
        var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                           ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!int.TryParse(userIdString, out int userId)) return Unauthorized();
        
        await user.UpdateTimezone(userId, request);
        return Ok();
    }

    [HttpGet("timezones")]
    [Authorize]
    public async Task<IActionResult> GetTimezones()
    {
        var response = await user.GetTimezones();
        return Ok(response);
    }
}
