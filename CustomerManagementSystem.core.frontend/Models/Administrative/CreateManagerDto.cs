using System.ComponentModel.DataAnnotations;

namespace CustomerManagementSystem.core.frontend.Models.Administrative
{
    public class CreateManagerDto
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [StringLength(128, ErrorMessage = "Họ và tên không được vượt quá 128 ký tự.")]
        public string OrgName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [StringLength(128, ErrorMessage = "Email không được vượt quá 128 ký tự.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string OrgEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [StringLength(16, ErrorMessage = "Số điện thoại không được vượt quá 16 ký tự.")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        public string OrgPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [StringLength(256, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có độ dài từ 6 đến 256 ký tự.")]
        public string OrgPassword { get; set; } = string.Empty;
    }
}
