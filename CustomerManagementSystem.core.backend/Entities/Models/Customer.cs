using System.ComponentModel.DataAnnotations;
using System;

namespace CustomerManagementSystem.core.backend.Entities.Models
{
    public class Customer
    {
        [Key]
        [Required]
        [MaxLength(128)]
        public string CustomerId { get; set; } = string.Empty; // Ma KH

        [Required]
        [StringLength(128)]
        public string CustomerName { get; set; } = string.Empty; // Ho va ten

        [Required]
        [StringLength(128)]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty; // Email

        [Required]
        [StringLength(16)]
        public string CustomerPhone { get; set; } = string.Empty; // So dien thoai

        [Required]
        public DateTime CustomerBirth { get; set; } // Ngay sinh

        [Required]
        [StringLength(128)]
        public string CustomerAddress { get; set; } = string.Empty; // Noi sinh song

        [Required]
        public bool IsActive { get; set; } = true; // Trang thai hoat dong

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Thoi diem luu KH

        public DateTime? DeletedAt { get; set; } // Thoi diem danh dau cho xoa boi Manager (null nghia la binh thuong)
    }
}