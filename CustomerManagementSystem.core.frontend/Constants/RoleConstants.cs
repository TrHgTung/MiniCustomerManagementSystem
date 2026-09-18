namespace CustomerManagementSystem.core.frontend.Constants
{
    /// <summary>
    /// Định nghĩa các hằng số về Role trong hệ thống
    /// </summary>
    public static class RoleConstants
    {
        /// <summary>
        /// Vai trò Manager: Yêu cầu thêm, sửa, xóa cần SA duyệt
        /// </summary>
        public const string Manager = "1";

        /// <summary>
        /// Vai trò Super Admin (SA): Toàn quyền duyệt, xóa vĩnh viễn, quản lý danh sách chờ xóa
        /// </summary>
        public const string SuperAdmin = "2";
    }
}
