using System.ComponentModel.DataAnnotations;
using System;

namespace CustomerManagementSystem.core.backend.Entities.Models
{
    public class Customer
    {
        [Key]
        [Required]
        [MaxLength(128)]
        public string CustomerId { get; set; } // Ma KH

        [Required]
        [StringLength(128)]
        public string CustomerName { get; set; } // Ho va ten

        [Required]
        [StringLength(128)]
        [EmailAddress]
        public string CustomerEmail { get; set; } // Email

        [Required]
        [StringLength(16)]
        public string CustomerPhone { get; set; } // So dien thoai

        [Required]
        public DateTime CustomerBirth { get; set; } // Ngay sinh

        [Required]
        [StringLength(128)]
        public string CustomerAddress { get; set; } // Noi sinh song

        [Required]
        public bool IsActive { get; set; } = true; // Trang thai hoat dong

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Thoi diem luu KH
    }
}