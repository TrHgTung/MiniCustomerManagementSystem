using System.ComponentModel.DataAnnotations;

namespace CustomerManagementSystem.core.frontend.Models.Auth
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string OrgEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        public string OrgPassword { get; set; } = string.Empty;
    }
}
