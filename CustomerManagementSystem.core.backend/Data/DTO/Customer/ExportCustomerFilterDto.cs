using System;
using System.ComponentModel.DataAnnotations;

namespace CustomerManagementSystem.core.backend.Data.DTO.Customer
{
    /// <summary>
    /// DTO chứa bộ lọc khoảng thời gian xuất báo cáo Excel cho dữ liệu Khách hàng
    /// </summary>
    public class ExportCustomerFilterDto
    {
        [Required(ErrorMessage = "Ngày bắt đầu (FromDate) là bắt buộc.")]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc (ToDate) là bắt buộc.")]
        public DateTime ToDate { get; set; }
    }
}
