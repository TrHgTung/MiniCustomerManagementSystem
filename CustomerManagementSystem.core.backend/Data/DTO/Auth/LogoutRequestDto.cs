using System.ComponentModel.DataAnnotations;

namespace CustomerManagementSystem.core.backend.Data.DTO.Auth
{
    public class LogoutRequestDto
    {
        [Required(ErrorMessage = "RefreshToken là bắt buộc.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
