using CustomerManagementSystem.core.backend.Entities.AppDataContext;
using CustomerManagementSystem.core.backend.Entities.Models;
using CustomerManagementSystem.core.backend.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementSystem.core.backend.Repositories.Implement
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == token);
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<bool> RevokeAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == token && r.IsActive);

            if (refreshToken == null)
                return false;

            refreshToken.IsActive = false;
            var result = await _context.SaveChangesAsync();
            if (result > 0) 
            {
                return true;
            }
            else return false;
        }

        public async Task<bool> RevokeByOrgIdAsync(string orgId)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(r => r.OrgId == orgId && r.IsActive)
                .ToListAsync();

            if (!activeTokens.Any())
                return false;

            foreach (var token in activeTokens)
            {
                token.IsActive = false;
            }

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
