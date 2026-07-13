using System.Linq.Expressions;
using Domain.Identity.Entities;
using Domain.Identity.Repositories;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Shared.Identity.Request;
using Shared.Response;
using Infrastructure.Common;

namespace Infrastructure.Identity.Repositories;

public class UserRepository(CashRegisterDbContext context) : IUserRepository
{
    public async Task CreateAsync(User entity) => await context.Users.AddAsync(entity);

    public async Task<User?> GetByIdAsync(int id) => await context.Users
        .Include(u => u.CashFlow)
        .Include(u => u.Person)
        .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate) =>
        await context.Users
            .Include(u => u.CashFlow)
            .Include(u => u.Person)
            .Where(predicate)
            .ToListAsync();

    public void Update(User entity) => context.Users.Update(entity);

    public void Delete(User entity) => context.Users.Remove(entity);

    public async Task<User?> GetUserByEmail(string email) => await context.Users
        .Include(u => u.Person)
        .FirstOrDefaultAsync(u => u.Person.Email == email);

    public async Task<User?> GetUserByUserName(string userName) => await context.Users
        .Include(u => u.Person)
        .FirstOrDefaultAsync(u => u.UserName == userName);

    public async Task<IEnumerable<User>> GetAllUsers() => await context.Users
        .Include(u => u.Person)
        .ToListAsync();

    public async Task<bool> UserExists(string userName, string email) => await context.Users
        .Include(u => u.Person)
        .AnyAsync(u => u.UserName == userName || u.Person.Email == email);

    public async Task<PagedResponse<User>> SearchAsync(SearchUserRequest request)
    {
        return await context.Users
            .Include(u => u.Person)
            .Where(u => string.IsNullOrWhiteSpace(request.Name) || 
                        u.Person.Name.FirstName.ToLower().Contains(request.Name.ToLower()) || 
                        u.Person.Name.LastName.ToLower().Contains(request.Name.ToLower()))
            .Where(u => string.IsNullOrWhiteSpace(request.TaxId) || 
                        u.Person.TaxId.Contains(request.TaxId))
            .Where(u => !request.BirthDate.HasValue || 
                        u.Person.Birthdate.Date == request.BirthDate.Value.Date)
            .OrderByDescending(u => u.Id)
            .ToPagedResponseAsync(request.Page, request.PageSize);
    }
}
