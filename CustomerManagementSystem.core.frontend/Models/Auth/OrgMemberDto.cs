namespace CustomerManagementSystem.core.frontend.Models.Auth
{
    public class OrgMemberDto
    {
        public string OrgId { get; set; } = string.Empty;
        public string OrgName { get; set; } = string.Empty;
        public string OrgEmail { get; set; } = string.Empty;
        public string OrgPhone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "2" = SA, "1" = Manager
        public bool IsActive { get; set; }
    }
}
