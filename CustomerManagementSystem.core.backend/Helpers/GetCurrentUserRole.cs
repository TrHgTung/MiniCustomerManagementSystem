using System.Security.Claims;

namespace CustomerManagementSystem.core.backend.Helpers
{
    /// <summary>
    /// Helper cung cấp tiện ích truy xuất Role của người dùng hiện tại từ Claims
    /// </summary>
    public static class UserRoleHelper
    {
        /// <summary>
        /// Lấy Role của user hiện tại từ ClaimsPrincipal (thuộc tính User trong ControllerBase)
        /// Mặc định trả về "1" (Manager) nếu không xác định được claim
        /// </summary>
        /// <param name="user">ClaimsPrincipal của phiên đăng nhập hiện tại</param>
        /// <returns>Role: "2" cho SA, "1" cho Manager</returns>
        public static string GetCurrentUserRole(this ClaimsPrincipal? user)
        {
            if (user == null)
            {
                return "1";
            }

            return user.FindFirst(ClaimTypes.Role)?.Value
                ?? user.FindFirst("role")?.Value
                ?? "1"; // mặc định là Manager (Role = "1")
        }
    }
}
