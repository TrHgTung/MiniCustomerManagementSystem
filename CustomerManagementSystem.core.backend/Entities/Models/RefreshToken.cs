using System.ComponentModel.DataAnnotations;
using System;

namespace CustomerManagementSystem.core.backend.Entities.Models
{
    public class RefreshToken
    {
        [Key]
        [Required]
        [MaxLength(128)]
        public int Id { get; set; } // so thu tu (ko can UUID)

        [Required]
        [StringLength(128)]
        public string Token { get; set; } = string.Empty; // Token from client

        [MaxLength(128)]
        public string? OrgId { get; set; }

        public OrgMember? OrgMember { get; set; }

        [Required]
        public bool IsActive { get; set; } = true; // Trang thai hoat dong

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // dựa vào đây để xác định thời gian hết hạn

    }
}