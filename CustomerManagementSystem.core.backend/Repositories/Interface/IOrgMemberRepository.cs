using CustomerManagementSystem.core.backend.Entities.Models;

namespace CustomerManagementSystem.core.backend.Repositories.Interface
{
    public interface IOrgMemberRepository
    {
        Task<OrgMember?> GetByEmailAsync(string email);
        Task<OrgMember?> GetByIdAsync(string id);
        Task<bool> HasAdminAccountAsync();
        Task<OrgMember> CreateAsync(OrgMember member);
        Task<bool> UpdateAsync(OrgMember member);
        Task<IEnumerable<OrgMember>> GetAllManagersAsync();
        Task<bool> ExistsByIdAsync(string id);
        Task<bool> ExistsByEmailAsync(string email);
    }
}
