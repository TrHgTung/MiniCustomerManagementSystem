using System.ComponentModel.DataAnnotations;

namespace CustomerManagementSystem.core.backend.Data.DTO.Auth
{
    public class LogoutRequestDto
    {
        public string? RefreshToken { get; set; }
    }
}
