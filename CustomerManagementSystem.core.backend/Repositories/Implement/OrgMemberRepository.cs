using CustomerManagementSystem.core.backend.Entities.AppDataContext;
using CustomerManagementSystem.core.backend.Entities.Models;
using CustomerManagementSystem.core.backend.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementSystem.core.backend.Repositories.Implement
{
    /// <summary>
    /// Role "2": account SA (Quản trị viên)
    /// Role "1": acc Manager (Quản lý)
    /// </summary>
    public class OrgMemberRepository : IOrgMemberRepository
    {

        private readonly ApplicationDbContext _context;

        public OrgMemberRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy thông tin organiztion member (theo email)
        public async Task<OrgMember?> GetByEmailAsync(string email)
        {
            return await _context.OrgMembers
                .FirstOrDefaultAsync(m => m.OrgEmail.ToLower() == email.ToLower());
        }

        // Lấy thông tin organiztion member (theo OrgId)
        public async Task<OrgMember?> GetByIdAsync(string id)
        {
            return await _context.OrgMembers
                .FirstOrDefaultAsync(m => m.OrgId == id);
        }

        // check role quyền của account
        public async Task<bool> HasAdminAccountAsync()
        {
            // nếu Role == "2" là tài khoản SA (Quản trị viên)
            return await _context.OrgMembers
                .AnyAsync(m => m.Role == "2");
        }

        // tạo account mới (chỉ có SA mới thực thi được)
        public async Task<OrgMember> CreateAsync(OrgMember member)
        {
            await _context.OrgMembers.AddAsync(member);
            await _context.SaveChangesAsync();
            return member;
        }

        // update account
        public async Task<bool> UpdateAsync(OrgMember member)
        {
            _context.OrgMembers.Update(member);
            var result = await _context.SaveChangesAsync();
            if (result)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
