using System;
using System.ComponentModel.DataAnnotations;

namespace CustomerManagementSystem.core.frontend.Models.Customer
{
    public class ExportCustomerFilterDto
    {
        [Required(ErrorMessage = "Ngày bắt đầu (FromDate) là bắt buộc.")]
        public DateTime FromDate { get; set; } = DateTime.Today.AddMonths(-1);

        [Required(ErrorMessage = "Ngày kết thúc (ToDate) là bắt buộc.")]
        public DateTime ToDate { get; set; } = DateTime.Today;
    }
}
