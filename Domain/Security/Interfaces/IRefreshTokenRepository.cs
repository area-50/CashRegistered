using Domain.Security.Entities;

namespace Domain.Security.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken);
    
    Task<RefreshToken?> GetByTokenAsync(string token);
    
    void Update(RefreshToken refreshToken);
}
