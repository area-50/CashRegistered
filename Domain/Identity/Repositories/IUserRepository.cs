using Domain.Identity.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.Response;

namespace Domain.Identity.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserByUserName(string userName);
    Task<IEnumerable<User>> GetAllUsers();
    Task<bool> UserExists(string userName, string email);
    Task<PagedResponse<User>> SearchAsync(SearchUserRequest request);
}