using System;

namespace CustomerManagementSystem.core.frontend.Models.Administrative
{
    public class ManagerDto
    {
        public string OrgId { get; set; } = string.Empty;
        public string OrgName { get; set; } = string.Empty;
        public string OrgEmail { get; set; } = string.Empty;
        public string OrgPhone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
