using CustomerManagementSystem.core.backend.Entities.Models;
using CustomerManagementSystem.core.backend.Repositories.Interface;
using CustomerManagementSystem.core.backend.Services.Interface;

namespace CustomerManagementSystem.core.backend.Services.Implement
{
    public class InitialSAAccountService : IInitialSAAccountService
    {
        private readonly IOrgMemberRepository _orgMemberRepository;
        private readonly ILogger<InitialSAAccountService> _logger;
        private readonly IConfiguration _configuration;

        public InitialSAAccountService(
            IOrgMemberRepository orgMemberRepository,
            ILogger<InitialSAAccountService> logger,
            IConfiguration configuration)
        {
            _orgMemberRepository = orgMemberRepository;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SeedAdminAccountAsync()
        {
            // Kiểm tra xem đã có tài khoản Quản trị viên (Role == "2") nào tồn tại chưa
            bool hasAdmin = await _orgMemberRepository.HasAdminAccountAsync();
            if (hasAdmin)
            {
                _logger.LogInformation("Đã có SA account");
                return;
            }

            // Đọc cấu hình tài khoản SA từ appsetting
            var saEmail = _configuration["InitialAdmin:Email"];
            var saPassword = _configuration["InitialAdmin:Password"];
            var saName = _configuration["InitialAdmin:Name"];
            var saPhone = _configuration["InitialAdmin:Phone"];
            var saId = _configuration["InitialAdmin:OrgId"];

            // Kiểm tra nếu không có cấu hình
            if (string.IsNullOrEmpty(saEmail) || string.IsNullOrEmpty(saPassword))
            {
                _logger.LogWarning("Không tìm thấy cấu hình tài khoản SA mặc định trong appsetting.json");
                return;
            }

            var adminAccount = new OrgMember
            {
                OrgId = saId,
                OrgName = saName,
                OrgEmail = saEmail.ToLowerInvariant(),
                OrgPhone = saPhone,
                OrgPassword = BCrypt.Net.BCrypt.HashPassword(saPassword),
                Role = "2",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _orgMemberRepository.CreateAsync(adminAccount);
            _logger.LogInformation("Đã khởi tạo thành công tài khoản SA : {saEmail}", saEmail);
        }
    }
}
