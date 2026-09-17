using CustomerManagementSystem.core.backend.Data.DTO.Administrative;
using CustomerManagementSystem.core.backend.Entities.Models;
using CustomerManagementSystem.core.backend.Repositories.Interface;
using CustomerManagementSystem.core.backend.Services.Interface;

namespace CustomerManagementSystem.core.backend.Services.Implement
{
    /// <summary>
    /// Service xử lý nghiệp vụ quản trị nhân sự cho SA (Super Admin - Role = "2")
    /// </summary>
    public class AdministrativeService : IAdministrativeService
    {
        private readonly IOrgMemberRepository _orgMemberRepository;
        private readonly ILogger<AdministrativeService> _logger;

        public AdministrativeService(
            IOrgMemberRepository orgMemberRepository,
            ILogger<AdministrativeService> logger)
        {
            _orgMemberRepository = orgMemberRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách toàn bộ tài khoản Manager (Role = "1")
        /// </summary>
        public async Task<IEnumerable<ManagerDto>> GetAllManagersAsync()
        {
            var managers = await _orgMemberRepository.GetAllManagersAsync();
            return managers.Select(MapToDto);
        }

        /// <summary>
        /// Lấy thông tin tài khoản Manager theo Id
        /// </summary>
        public async Task<ManagerDto?> GetManagerByIdAsync(string id)
        {
            var member = await _orgMemberRepository.GetByIdAsync(id);
            if (member == null || member.Role != "1")
            {
                return null;
            }

            return MapToDto(member);
        }

        /// <summary>
        /// Tạo mới tài khoản Manager (Role = "1", IsActive = true)
        /// Mật khẩu được mã hóa an toàn bằng BCrypt
        /// </summary>
        public async Task<ManagerDto> CreateManagerAsync(CreateManagerDto createDto)
        {
            var email = createDto.OrgEmail.Trim().ToLowerInvariant();

            // check trùng Email đã tồn tại hay chưa
            if (await _orgMemberRepository.ExistsByEmailAsync(email))
            {
                throw new InvalidOperationException($"Email '{createDto.OrgEmail}' đã được sử dụng trong hệ thống.");
            }

            // Khởi tạo đối tượng OrgMember với Role = "1" (Manager) và IsActive = true
            var manager = new OrgMember
            {
                OrgId = Guid.NewGuid().ToString(),
                OrgName = createDto.OrgName.Trim(),
                OrgEmail = email,
                OrgPhone = createDto.OrgPhone.Trim(),
                OrgPassword = BCrypt.Net.BCrypt.HashPassword(createDto.OrgPassword),
                Role = "1", // luôn luôn là sinh ra account Manager, vì SA chỉ có 1 theo đefault
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _orgMemberRepository.CreateAsync(manager);
            _logger.LogInformation("SA đã tạo thành công tài khoản Manager: {OrgId} - {OrgEmail}", manager.OrgId, manager.OrgEmail);

            return MapToDto(manager);
        }

        /// <summary>
        /// Vô hiệu hóa (soft delêt) tài khoản Manager: đặt IsActive = false
        /// </summary>
        public async Task<bool> DeactivateManagerAsync(string id)
        {
            var member = await _orgMemberRepository.GetByIdAsync(id);
            if (member == null)
            {
                return false;
            }

            // tuyệt đối không cho phép vô hiệu hóa tài khoản SA account
            if (member.Role == "2")
            {
                throw new InvalidOperationException("Không thể vô hiệu hóa tài khoản Quản trị viên cấp cao (SA).");
            }

            // Chỉ vô hiệu hóa tài khoản Manager (Role = "1")
            if (member.Role != "1")
            {
                throw new InvalidOperationException("Tài khoản này không phải là Manager.");
            }

            member.IsActive = false;
            var updated = await _orgMemberRepository.UpdateAsync(member);
            if (updated)
            {
                _logger.LogInformation("SA đã vô hiệu hóa tài khoản Manager: {OrgId} ({OrgEmail})", member.OrgId, member.OrgEmail);
            }

            return updated;
        }

        /// <summary>
        /// Mapper chuyển từ Entity OrgMember sang DTO ManagerDto (ẩn mật khẩu)
        /// </summary>
        private static ManagerDto MapToDto(OrgMember member)
        {
            return new ManagerDto
            {
                OrgId = member.OrgId,
                OrgName = member.OrgName,
                OrgEmail = member.OrgEmail,
                OrgPhone = member.OrgPhone,
                Role = member.Role,
                IsActive = member.IsActive,
                CreatedAt = member.CreatedAt
            };
        }
    }
}
