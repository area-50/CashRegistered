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
        var token = response.AccessToken;
        if (token.Length <= 0) return Unauthorized();
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(30)
        };

        Response.Cookies.Append("access_token", token, cookieOptions);
        
        return Ok(new {response.Id, response.UserName, response.Role});
    }
    
    [HttpPost("logout")]
    [AllowAnonymous] 
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token", new CookieOptions
        {
            Path = "/"
        });

        return Ok(new { message = "Sessão encerrada com sucesso." });
    }
    
    [HttpGet("verify")]
    [Authorize]
    public IActionResult Verify()
    {
        return Ok(); 
    }
}