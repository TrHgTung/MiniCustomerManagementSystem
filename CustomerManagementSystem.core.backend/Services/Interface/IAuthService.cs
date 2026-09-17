using CustomerManagementSystem.core.backend.Data.DTO.Auth;

namespace CustomerManagementSystem.core.backend.Services.Interface
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<bool> LogoutAsync(LogoutRequestDto request);
        Task<OrgMemberDto?> GetProfileAsync(string orgId);
    }
}
