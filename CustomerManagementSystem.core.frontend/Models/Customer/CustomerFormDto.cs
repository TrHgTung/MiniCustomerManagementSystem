using System;
using System.ComponentModel.DataAnnotations;

namespace CustomerManagementSystem.core.frontend.Models.Customer
{
    public class CustomerFormDto
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [StringLength(128, ErrorMessage = "Họ và tên không được vượt quá 128 ký tự.")]
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

        [Required(ErrorMessage = "Nơi sinh sống (Tỉnh/Thành) là bắt buộc.")]
        [StringLength(128, ErrorMessage = "Nơi sinh sống không được vượt quá 128 ký tự.")]
        public string CustomerAddress { get; set; } = string.Empty;
    }
}
