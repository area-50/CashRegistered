using Application.Identity.Interfaces;
using Application.Security.Interfaces;
using Application.Financial.Interfaces;
using Domain.Security.Interfaces;
using Flunt.Notifications;
using Shared.Identity.Request;
using Shared.Security.Request;
using Shared.Financial.Request;
using Shared.Response;
using Shared.Identity.Response;
using Shared.Security.Response;
using Shared.Financial.Response;
using Shared.Notifications;
using Shared.Abstractions;
using Domain.Security.Entities;


namespace Application.Security.UseCases;

public class AuthAppService(
    IUserUseCase userUseCase,
    ITokenGenerator tokenGenerator,
    IPasswordHasher hasher,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    NotificationContext notificationContext
) : IAuthAppService
{
    public async Task<LoginUserResponse> Login(LoginRequest request)
    {
        var user = await userUseCase.GetUserLoginByUserName(request.UserName);

        if (user is null)
        {
            return new LoginUserResponse();
        }

        if (user.AuthenticatePassword(hasher, request.Password))
        {
            var refreshToken = new RefreshToken(GenerateRefreshToken(), user.Id, DateTime.UtcNow.AddDays(7));
            await refreshTokenRepository.AddAsync(refreshToken);
            await unitOfWork.CommitAsync();

            return new LoginUserResponse
            {
                AccessToken = tokenGenerator.GenerateToken(user),
                RefreshToken = refreshToken.Token,
                UserName = user.UserName,
                Name = user.Person.Name,
                Role = user.UserRole.ToString()
            };
        }
        
        notificationContext.AddNotifications(user.Notifications);
        return new LoginUserResponse();
    }

    public async Task<LoginUserResponse> RefreshToken(string token)
    {
        var existingToken = await refreshTokenRepository.GetByTokenAsync(token);
        if (existingToken is null || !existingToken.IsValid)
        {
            notificationContext.AddNotification("Token", "Refresh token inválido ou expirado.");
            return new LoginUserResponse();
        }

        existingToken.Revoke();
        refreshTokenRepository.Update(existingToken);

        var newRefreshToken = new RefreshToken(GenerateRefreshToken(), existingToken.UserId, DateTime.UtcNow.AddDays(7));
        await refreshTokenRepository.AddAsync(newRefreshToken);
        await unitOfWork.CommitAsync();

        return new LoginUserResponse
        {
            AccessToken = tokenGenerator.GenerateToken(existingToken.User),
            RefreshToken = newRefreshToken.Token,
            UserName = existingToken.User.UserName,
            Name = existingToken.User.Person.Name,
            Role = existingToken.User.UserRole.ToString()
        };
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
    }
}