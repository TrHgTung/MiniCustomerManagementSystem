using System.Security.Claims;
using CustomerManagementSystem.core.frontend.Constants;
using CustomerManagementSystem.core.frontend.Models.Auth;

namespace CustomerManagementSystem.core.frontend.Helpers
{
    /// <summary>
    /// Các phương thức mở rộng và tiện ích kiểm tra vai trò (Role) người dùng tập trung,
    /// tái sử dụng ở toàn bộ các component giao diện và service.
    /// </summary>
    public static class UserRoleHelper
    {
        /// <summary>
        /// Kiểm tra ClaimsPrincipal hiện tại có phải vai trò Super Admin (SA - Role="2") không
        /// </summary>
        public static bool IsSuperAdmin(this ClaimsPrincipal? user)
        {
            if (user == null) return false;

            return user.IsInRole(RoleConstants.SuperAdmin)
                || user.HasClaim(ClaimTypes.Role, RoleConstants.SuperAdmin)
                || user.HasClaim("role", RoleConstants.SuperAdmin);
        }

        /// <summary>
        /// Kiểm tra thông tin OrgMemberDto hiện tại có phải vai trò Super Admin (SA - Role="2") không
        /// </summary>
        public static bool IsSuperAdmin(this OrgMemberDto? member)
        {
            return member?.Role == RoleConstants.SuperAdmin;
        }

        /// <summary>
        /// Kiểm tra ClaimsPrincipal hiện tại có phải vai trò Manager (Role="1") không
        /// </summary>
        public static bool IsManager(this ClaimsPrincipal? user)
        {
            if (user == null) return false;

            return user.IsInRole(RoleConstants.Manager)
                || user.HasClaim(ClaimTypes.Role, RoleConstants.Manager)
                || user.HasClaim("role", RoleConstants.Manager);
        }

        /// <summary>
        /// Kiểm tra thông tin OrgMemberDto hiện tại có phải vai trò Manager (Role="1") không
        /// </summary>
        public static bool IsManager(this OrgMemberDto? member)
        {
            return member?.Role == RoleConstants.Manager;
        }

        /// <summary>
        /// Kiểm tra người dùng có phải SA dựa trên cả OrgMemberDto và ClaimsPrincipal
        /// </summary>
        public static bool CheckIsSuperAdmin(OrgMemberDto? member, ClaimsPrincipal? user)
        {
            return member.IsSuperAdmin() || user.IsSuperAdmin();
        }

        /// <summary>
        /// Lấy Role string ("2" hoặc "1") từ ClaimsPrincipal, mặc định "1" nếu không tìm thấy
        /// </summary>
        public static string GetCurrentUserRole(this ClaimsPrincipal? user)
        {
            if (user == null) return RoleConstants.Manager;

            return user.FindFirst(ClaimTypes.Role)?.Value
                ?? user.FindFirst("role")?.Value
                ?? RoleConstants.Manager;
        }
    }
}
