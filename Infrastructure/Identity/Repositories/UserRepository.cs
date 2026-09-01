using System.Linq.Expressions;
using Domain.Identity.Entities;
using Domain.Identity.Repositories;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Infrastructure.Utils.Interfaces;
using Domain.Shared.DTOs;
using Domain.Shared.Response;
using Infrastructure.Common;

namespace Infrastructure.Identity.Repositories;

public class UserRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : IUserRepository
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
        var query = context.Users
            .Include(u => u.Person)
            .AsNoTracking();

        query = sqlUtils.WhereAnd(
            query, !string.IsNullOrWhiteSpace(request.Name),
            u => EF.Functions.ILike(u.Person.Name.FirstName, sqlUtils.SqlLikeContains(request.Name!.Trim())) ||
                 EF.Functions.ILike(u.Person.Name.LastName, sqlUtils.SqlLikeContains(request.Name!.Trim()))
        );

        query = sqlUtils.WhereAnd(
            query, !string.IsNullOrWhiteSpace(request.TaxId),
            u => EF.Functions.ILike(u.Person.TaxId, sqlUtils.SqlLikeContains(request.TaxId!.Trim()))
        );

        query = sqlUtils.WhereAnd(
            query, request.BirthDate.HasValue,
            u => u.Person.Birthdate.Date == request.BirthDate!.Value.Date
        );

        return await query
            .OrderByDescending(u => u.Id)
            .ToPagedResponseAsync(request.Page, request.PageSize);
    }
}
