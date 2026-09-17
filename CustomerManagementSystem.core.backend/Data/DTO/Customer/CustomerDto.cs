using System;

namespace CustomerManagementSystem.core.backend.Data.DTO.Customer
{
    /// <summary>
    /// DTO này dùng để "truyền dữ liệu giữa Controller và Service", bảo mật data trước khi trả lên cho client
    /// </summary>
    public class CustomerDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public DateTime CustomerBirth { get; set; }
        public string CustomerAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
