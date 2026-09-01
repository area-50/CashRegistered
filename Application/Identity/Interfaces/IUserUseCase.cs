using Domain.Identity.Entities;
using Domain.Shared.DTOs;
using Shared.Security.Request;
using Shared.Identity.Response;
using Domain.Shared.Response;

namespace Application.Identity.Interfaces;

public interface IUserUseCase
{
    Task<CreateResponse> CreateUser(CreateUserRequest userRequest, CreatePersonRequest? personRequest);
    
    Task DeactivateUser(int userId);
    
    Task UpdateTimezone(int userId, UpdateTimezoneRequest request);
    
    Task ChangePassword(int userId, ChangePasswordRequest request);
    
    Task <IEnumerable<GetAllUsersResponse>> GetAllUsers();
    
    Task<User?> GetUserById(int userId);
    
    Task<User> GetValidUserById(int userId);
    
    Task<User> GetValidUserByEmail(string email);
    
    Task<User?> GetUserByUserName(string userName);
    
    Task<User?> GetUserLoginByUserName(string userName);
    
    Task<PagedResponse<GetAllUsersResponse>> SearchUsers(SearchUserRequest request);
    
    Task<GetMeResponse> GetMe(int userId);
    
    Task<IEnumerable<GetTimezoneResponse>> GetTimezones();
}