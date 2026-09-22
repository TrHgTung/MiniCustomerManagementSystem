using System.ComponentModel.DataAnnotations;

namespace CustomerManagementSystem.core.backend.Entities.Models
{
    public class Idempotency
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Key { get; set; } = null!;

        [Required]
        [MaxLength(512)]
        public string RequestPath { get; set; } = null!;

        public int StatusCode { get; set; }

        public string ResponseBody { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}