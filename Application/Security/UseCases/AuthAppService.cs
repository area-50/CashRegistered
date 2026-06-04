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


namespace Application.Security.UseCases;

public class AuthAppService(
    IUserUseCase userUseCase,
    ITokenGenerator tokenGenerator,
    IPasswordHasher hasher,
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
            return new LoginUserResponse
            {
                AccessToken = tokenGenerator.GenerateToken(user),
                Id = user.Id,
                UserName = user.Person.Name,
                Role = user.UserRole.ToString()
            };
        notificationContext.AddNotifications(user.Notifications);
        return new LoginUserResponse();
    }
}