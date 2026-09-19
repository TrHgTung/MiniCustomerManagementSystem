using System.ComponentModel.DataAnnotations;
using System;

namespace CustomerManagementSystem.core.backend.Entities.Models
{
    public class OrgMember
    {
        [Key]
        [Required]
        [MaxLength(128)]
        public string OrgId { get; set; } = string.Empty; // Ma nhan su

        [Required]
        [StringLength(128)]
        public string OrgName { get; set; } = string.Empty; // Ho va ten

        [Required]
        [StringLength(128)]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string OrgEmail { get; set; } = string.Empty; // Email

        [Required]
        [StringLength(16)]
        public string OrgPhone { get; set; } = string.Empty; // So dien thoai

        [Required]
        [StringLength(256)]
        public string OrgPassword { get; set; } = string.Empty; // Mat khau

        [Required]
        [StringLength(8)]
        public string Role { get; set; } = string.Empty; // vai tro (Manager / SA)

        [Required]
        public bool IsActive { get; set; } = true; // Trang thai hoat dong

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Thoi diem luu thong tin nhan su
    }
}