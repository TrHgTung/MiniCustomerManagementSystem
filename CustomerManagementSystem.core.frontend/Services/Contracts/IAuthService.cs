using CustomerManagementSystem.core.frontend.Models.Auth;

namespace CustomerManagementSystem.core.frontend.Services.Contracts
{
    /// <summary>
    /// Service for authentication operations.
    /// </summary>
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<OrgMemberDto?> GetCurrentUserInfoAsync();
    }
}
