using Application.Identity.Interfaces;
using Domain.Identity.Entities;
using Domain.Identity.Enums;
using Domain.Security.Interfaces;
using Domain.Identity.Repositories;
using Domain.Shared.Abstractions;
using Domain.Shared.DTOs;
using Shared.Security.Request;
using Shared.Identity.Response;
using Domain.Shared.Response;
using Domain.Shared.Notifications;
using Domain.Shared.Validations;

namespace Application.Identity.UseCases;

public class UserUseCase(
    IUserRepository repository,
    IPersonRepository personRepository,
    IPersonUseCase personUseCase,
    IUnitOfWork unitOfWork,
    IPasswordHasher hashServices,
    NotificationContext notificationContext
) : GeneralValidator, IUserUseCase
{
    public async Task<CreateResponse> CreateUser(
        CreateUserRequest userRequest,
        CreatePersonRequest? personRequest
    )
    {
        int personId;

        if (userRequest.PersonId > 0)
        {
            var person = await personRepository.GetByIdAsync(userRequest.PersonId.Value);
            if (person == null)
            {
                notificationContext.AddNotification("Person", "A pessoa informada não existe.");
                return new CreateResponse();
            }
            personId = person.Id;
        }
        else
        {
            if (personRequest == null)
            {
                notificationContext.AddNotification(
                    "Person", "Os dados da pessoa são obrigatórios para um novo cadastro."
                );
                return new CreateResponse();
            }

            var createPersonRequest = new CreatePersonRequest
            {
                FirstName = personRequest.FirstName,
                LastName = personRequest.LastName,
                Birthdate = personRequest.Birthdate,
                TaxId = personRequest.TaxId,
                Email = personRequest.Email,
                CellPhone = personRequest.CellPhone!,
                Phone = personRequest.Phone!,
                Gender = personRequest.Gender!,
                PersonType = "Physical" // Default to Physical for User creation if not specified
            };

            var personResponse = await personUseCase.CreatePerson(createPersonRequest);
            if (personResponse.Id == 0)
            {
                return new CreateResponse();
            }
            personId = personResponse.Id;
        }
        
        var existingUserByUsername = await repository.GetUserByUserName(userRequest.UserName);
        var usersForPerson = await repository.FindAsync(u => u.PersonId == personId);
        var personAlreadyHasUser = usersForPerson.Any();
        
        var userRole = Enum.TryParse(userRequest.Role, out UserRole role) ? role : UserRole.Business;
        
        User user = new (
            personId,
            userRequest.Password,
            userRequest.UserName,
            userRole
        );
        
        user.ValidateUniqueUser(existingUserByUsername != null, personAlreadyHasUser);

        if (user.IsInvalid)
        {
            notificationContext.AddNotifications(user.Notifications);
            return new CreateResponse();
        }

        user.HashPassword(hashServices);
        
        await repository.CreateAsync(user);
        await unitOfWork.CommitAsync();

        return new CreateResponse { Id = user.Id };
    }
    
    public async Task DeactivateUser(int userId)
    {
        var user = await repository.GetByIdAsync(userId);
        if (User.NotExists(user, notificationContext)) return;
        
        user!.Deactivate();
        
        repository.Update(user);
        await unitOfWork.CommitAsync();
    }

    public async Task ChangePassword(int userId, ChangePasswordRequest request)
    {
        var user = await repository.GetByIdAsync(userId);
        if (User.NotExists(user, notificationContext)) return;

        if (!user!.AuthenticatePassword(hashServices, request.OldPassword))
        {
            notificationContext.AddNotifications(user.Notifications);
            return;
        }

        user.UpdatePassword(request.NewPassword, hashServices);

        if (user.IsInvalid)
        {
            notificationContext.AddNotifications(user.Notifications);
            return;
        }

        repository.Update(user);
        await unitOfWork.CommitAsync();
    }

    public async Task<IEnumerable<GetAllUsersResponse>> GetAllUsers()
    {
        var allUsers = await repository.FindAsync(u => u.IsActive);
        var selectedUsers = allUsers.Select(
            user => new GetAllUsersResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Role = user.UserRole.ToString(),
                Name = user.Person.Name,
                Birthdate = user.Person.Birthdate,
                TaxId = user.Person.TaxId,
                IsActive = user.IsActive
            }
        );
        
        return selectedUsers;
    }

    public async Task<IEnumerable<User>> GetUsersIncludeCashFlow()
    {
        return await repository.FindAsync(u => true);
    }

    public async Task<User?> GetUserById(int userId)
    {
        return await repository.GetByIdAsync(userId);
    }

    public async Task<User> GetValidUserById(int userId)
    {
        var user = await GetUserById(userId);
        User.ValidateUserExists(user, notificationContext);
        return user!;
    }

    public async Task<User> GetValidUserByEmail(string email)
    {
        var user = await repository.GetUserByEmail(email);
        User.ValidateUserExists(user, notificationContext);
        return user!;
    }

    public async Task<User> GetValidUserByUserName(string userName)
    {
        var user = await repository.GetUserByUserName(userName);
        User.ValidateUserExists(user, notificationContext);
        return user!;
    }

    public async Task<PagedResponse<GetAllUsersResponse>> SearchUsers(SearchUserRequest request)
    {
        var pagedUsers = await repository.SearchAsync(request);
        
        return new PagedResponse<GetAllUsersResponse>
        {
            Items = pagedUsers.Items.Select(u => new GetAllUsersResponse
            {
                Id = u.Id,
                UserName = u.UserName,
                Role = u.UserRole.ToString(),
                Name = u.Person.Name,
                Birthdate = u.Person.Birthdate,
                TaxId = u.Person.TaxId,
                IsActive = u.IsActive
            }),
            TotalCount = pagedUsers.TotalCount,
            Page = pagedUsers.Page,
            PageSize = pagedUsers.PageSize
        };
    }

    public async Task UpdateUserProfile(int userId, Shared.Identity.Request.UpdateUserProfileRequest request)
    {
        var user = await repository.GetByIdAsync(userId);
        if (User.NotExists(user, notificationContext)) return;

        user!.Person.Update(
            personType: user.Person.PersonType,
            firstName: request.FirstName,
            lastName: request.LastName,
            taxId: user.Person.TaxId,
            birthdate: request.Birthdate,
            email: request.Email,
            cellPhone: request.CellPhone,
            phone: request.Phone,
            gender: request.Gender
        );

        if (user.Person.IsInvalid || user.IsInvalid)
        {
            notificationContext.AddNotifications(user.Person.Notifications);
            notificationContext.AddNotifications(user.Notifications);
            return;
        }

        repository.Update(user);
        await unitOfWork.CommitAsync();
    }

    public async Task AdminUpdateUser(int targetUserId, Shared.Identity.Request.AdminUpdateUserRequest request)
    {
        var user = await repository.GetByIdAsync(targetUserId);
        if (User.NotExists(user, notificationContext)) return;

        var existingUserByUsername = await repository.GetUserByUserName(request.UserName);
        if (existingUserByUsername != null && existingUserByUsername.Id != targetUserId)
        {
            notificationContext.AddNotification("UserName", "Este nome de usuário já está em uso por outro usuário.");
            return;
        }

        var newRole = Enum.TryParse(request.Role, out UserRole role) ? role : user!.UserRole;
        user!.AdminUpdateUser(newRole, request.UserName);

        if (request.IsActive && !user.IsActive) user.Activate();
        else if (!request.IsActive && user.IsActive) user.Deactivate();

        user.Person.Update(
            personType: user.Person.PersonType,
            firstName: request.FirstName,
            lastName: request.LastName,
            taxId: user.Person.TaxId,
            birthdate: request.Birthdate,
            email: request.Email,
            cellPhone: request.CellPhone,
            phone: request.Phone,
            gender: request.Gender
        );

        if (user.IsInvalid || user.Person.IsInvalid)
        {
            notificationContext.AddNotifications(user.Notifications);
            notificationContext.AddNotifications(user.Person.Notifications);
            return;
        }

        repository.Update(user);
        await unitOfWork.CommitAsync();
    }

    public async Task AdminResetPassword(int targetUserId, Shared.Identity.Request.AdminResetPasswordRequest request)
    {
        var user = await repository.GetByIdAsync(targetUserId);
        if (User.NotExists(user, notificationContext)) return;

        user!.UpdatePassword(request.NewPassword, hashServices);

        if (user.IsInvalid)
        {
            notificationContext.AddNotifications(user.Notifications);
            return;
        }

        repository.Update(user);
        await unitOfWork.CommitAsync();
    }

    public async Task UpdateTimezone(int userId, UpdateTimezoneRequest request)
    {
        var user = await repository.GetByIdAsync(userId);
        if (User.NotExists(user, notificationContext)) return;

        user!.UpdateTimezone(request.Timezone);

        if (user.IsInvalid)
        {
            notificationContext.AddNotifications(user.Notifications);
            return;
        }

        repository.Update(user);
        await unitOfWork.CommitAsync();
    }

    public async Task<GetMeResponse> GetMe(int userId)
    {
        var user = await repository.GetByIdAsync(userId);
        if (User.NotExists(user, notificationContext)) return new GetMeResponse();

        return new GetMeResponse
        {
            UserName = user!.UserName,
            Name = user.Person.Name,
            Role = user.UserRole.ToString(),
            Timezone = user.Timezone,
            Birthdate = user.Person.Birthdate,
            TaxId = user.Person.TaxId,
            Email = user.Person.Email,
            CellPhone = user.Person.CellPhone,
            Phone = user.Person.Phone,
            Gender = user.Person.Gender.ToString()
        };
    }

    public async Task<GetUserByIdResponse?> GetUserByIdResponse(int userId)
    {
        var user = await repository.GetByIdAsync(userId);
        if (User.NotExists(user, notificationContext)) return null;

        return new GetUserByIdResponse
        {
            Id = user!.Id,
            UserName = user.UserName,
            Role = user.UserRole.ToString(),
            IsActive = user.IsActive,
            Name = user.Person.Name,
            Birthdate = user.Person.Birthdate,
            TaxId = user.Person.TaxId,
            Email = user.Person.Email,
            CellPhone = user.Person.CellPhone,
            Phone = user.Person.Phone,
            Gender = user.Person.Gender.ToString()
        };
    }

    public async Task<User?> GetUserByUserName(string userName)
    {
        return await repository.GetUserByUserName(userName);
    }

    public async Task<User?> GetUserLoginByUserName(string userName)
    {
        var user = await repository.GetUserByUserName(userName);
        User.ValidateUserLoginExists(user, notificationContext);
        return user;
    }

    public async Task<IEnumerable<GetTimezoneResponse>> GetTimezones()
    {
        var timezones = TimeZoneInfo.GetSystemTimeZones()
            .Select(tz => new GetTimezoneResponse
            {
                Id = tz.Id,
                DisplayName = tz.DisplayName
            });

        return await Task.FromResult(timezones);
    }
}
