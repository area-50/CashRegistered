using Domain.Identity.Entities;
using Shared.Identity.Request;
using Shared.Security.Request;
using Shared.Identity.Response;
using Shared.Response;

namespace Application.Identity.Interfaces;

public interface IUserUseCase
{
    Task<CreateResponse> CreateUser(CreateUserRequest userRequest, CreatePersonRequest? personRequest);
    Task DeactivateUser(int userId);
    Task ChangePassword(int userId, ChangePasswordRequest request);
    Task <IEnumerable<GetAllUsersResponse>> GetAllUsers();
    Task<User?> GetUserById(int userId);
    Task<User> GetValidUserById(int userId);
    Task<User> GetValidUserByEmail(string email);
    Task<User?> GetUserByUserName(string userName);
    Task<User?> GetUserLoginByUserName(string userName);
    Task<PagedResponse<GetAllUsersResponse>> SearchUsers(SearchUserRequest request);
}