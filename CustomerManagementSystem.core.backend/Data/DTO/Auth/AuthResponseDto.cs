using System;

namespace CustomerManagementSystem.core.backend.Data.DTO.Auth
{
    /// <summary>
    /// DTO này dùng để "truyền dữ liệu từ Service lên Controller", 
    /// trả về cho client sau khi đăng nhập thành công
    /// </summary>
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public OrgMemberDto User { get; set; } = null!;
    }
}
