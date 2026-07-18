using Shared.Security.Request;
using Shared.Security.Response;

namespace Application.Security.Interfaces;

public interface IAuthAppService
{
    Task<LoginUserResponse> Login(LoginRequest request);
    
    Task<LoginUserResponse> RefreshToken(string token);
}