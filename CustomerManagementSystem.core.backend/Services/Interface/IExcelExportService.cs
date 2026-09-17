using System;

namespace CustomerManagementSystem.core.backend.Services.Interface
{
    /// <summary>
    /// Service phụ trách xuất dữ liệu ra định dạng bảng tính Excel (.xlsx)
    /// </summary>
    public interface IExcelExportService
    {
        /// <summary>
        /// Xuất danh sách khách hàng trong khoảng thời gian (CreatedAt) ra file Excel byte[]
        /// </summary>
        Task<byte[]> ExportCustomersToExcelAsync(DateTime fromDate, DateTime toDate, string userRole);
    }
}
