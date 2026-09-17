using System.ComponentModel.DataAnnotations;
using System;

namespace CustomerManagementSystem.core.backend.Entities.Models
{
    public class OrgMember
    {
        [Key]
        [Required]
        [MaxLength(128)]
        public string OrgId { get; set; } // Ma nhan su

        [Required]
        [StringLength(128)]
        public string OrgName { get; set; } // Ho va ten

        [Required]
        [StringLength(128)]
        [EmailAddress]
        public string OrgEmail { get; set; } // Email

        [Required]
        [StringLength(16)]
        public string OrgPhone { get; set; } // So dien thoai

        [Required]
        [StringLength(256)]
        public string OrgPassword { get; set; } // Mat khau

        [Required]
        [StringLength(8)]
        public string Role { get; set; } // vai tro (Manager / SA)

        [Required]
        public bool IsActive { get; set; } = true; // Trang thai hoat dong

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Thoi diem luu thong tin nhan su
    }
}