using CustomerManagementSystem.core.frontend.Models.Auth;

namespace CustomerManagementSystem.core.frontend.Services.Contracts
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<OrgMemberDto?> GetCurrentUserInfoAsync();
    }
}
