using CustomerManagementSystem.core.backend.Entities.Models;

namespace CustomerManagementSystem.core.backend.Repositories.Interface
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
        Task<bool> RevokeAsync(string token);
    }
}
