using Domain.Security.Entities;
using Domain.Security.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Security.Repositories;

public class RefreshTokenRepository(CashRegisterDbContext context) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken refreshToken)
    {
        await context.RefreshTokens.AddAsync(refreshToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public void Update(RefreshToken refreshToken)
    {
        context.RefreshTokens.Update(refreshToken);
    }
}
