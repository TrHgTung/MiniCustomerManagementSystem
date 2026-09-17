using System.ComponentModel.DataAnnotations;

namespace CustomerManagementSystem.core.backend.Data.DTO.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Thiếu AccessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thiếu RefreshToken")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
