using Application.Security.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Security.Request;

namespace CashRegister.Controllers.Security;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthAppService authService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await authService.Login(request);
        
        if (string.IsNullOrEmpty(response.AccessToken))
        {
            return BadRequest();
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(30)
        };

        Response.Cookies.Append("access_token", response.AccessToken, cookieOptions);
        Response.Cookies.Append("refresh_token", response.RefreshToken, cookieOptions);
        
        return Ok(new Shared.Response.ApiResponse<object>
        {
            Data = new { response.UserName, response.Name, response.Role }
        });
    }
    
    [HttpPost("logout")]
    [AllowAnonymous] 
    public IActionResult Logout()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        };

        Response.Cookies.Delete("access_token", cookieOptions);
        Response.Cookies.Delete("refresh_token", cookieOptions);

        return Ok(new { message = "Sessão encerrada com sucesso." });
    }
    
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
        {
            return BadRequest(new { message = "Refresh token não fornecido." });
        }

        var response = await authService.RefreshToken(refreshToken);

        if (string.IsNullOrEmpty(response.AccessToken))
        {
            return Unauthorized(new { message = "Refresh token inválido ou expirado." });
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(30)
        };

        Response.Cookies.Append("access_token", response.AccessToken, cookieOptions);
        Response.Cookies.Append("refresh_token", response.RefreshToken, cookieOptions);

        return Ok(new Shared.Response.ApiResponse<object>
        {
            Data = new { response.UserName, response.Name, response.Role }
        });
    }
    
    [HttpGet("verify")]
    [Authorize]
    public IActionResult Verify()
    {
        return Ok(); 
    }
}