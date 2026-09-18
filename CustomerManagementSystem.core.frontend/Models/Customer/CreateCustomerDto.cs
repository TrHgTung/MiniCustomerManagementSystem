using System;
using System.ComponentModel.DataAnnotations;

namespace CustomerManagementSystem.core.frontend.Models.Customer
{
    public class CreateCustomerDto
    {
        [Required(ErrorMessage = "Tên khách hàng là bắt buộc.")]
        [StringLength(128, ErrorMessage = "Tên khách hàng không được vượt quá 128 ký tự.")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [StringLength(128, ErrorMessage = "Email không được vượt quá 128 ký tự.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [StringLength(16, ErrorMessage = "Số điện thoại không được vượt quá 16 ký tự.")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
        public DateTime CustomerBirth { get; set; } = DateTime.Today.AddYears(-20);

        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        [StringLength(128, ErrorMessage = "Địa chỉ không được vượt quá 128 ký tự.")]
        public string CustomerAddress { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
