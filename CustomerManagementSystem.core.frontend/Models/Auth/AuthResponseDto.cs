using System;

namespace CustomerManagementSystem.core.frontend.Models.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public OrgMemberDto User { get; set; } = new();
    }
}
