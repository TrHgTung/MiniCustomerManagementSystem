using CustomerManagementSystem.core.backend.Data.DTO.Administrative;
using CustomerManagementSystem.core.backend.Helpers.Attributes;
using CustomerManagementSystem.core.backend.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CustomerManagementSystem.core.backend.Controllers.Admin
{
    /// <summary>
    /// Administrative Controller: api/v1/admin/administrative
    /// Quản lý tài khoản nhân sự / quản lý (Manager - Role = "1")
    /// Chuc nang nay chi danh cho SA accont
    /// </summary>
    [EnableRateLimiting("Admin")]
    [Authorize(Policy = "AdminOnly")]
    [ApiController]
    [Route("admin")]
    public class AdministrativeController : ControllerBase
    {
        private readonly IAdministrativeService _administrativeService;
        private readonly ILogger<AdministrativeController> _logger;

        public AdministrativeController(
            IAdministrativeService administrativeService,
            ILogger<AdministrativeController> logger)
        {
            _administrativeService = administrativeService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách toàn bộ tài khoản Manager (Role = "1")
        /// Chỉ SA (Role = "2") mới có quyền gọi API này
        /// </summary>
        [HttpGet("managers")]
        public async Task<ActionResult<IEnumerable<ManagerDto>>> GetAllManagers()
        {
            var managers = await _administrativeService.GetAllManagersAsync();
            return Ok(managers);
        }

        /// <summary>
        /// Lấy chi tiết thông tin tài khoản Manager theo mã nhân sự (id)
        /// Chỉ SA (Role = "2") mới có quyền gọi API này
        /// </summary>
        [HttpGet("managers/{id}")]
        public async Task<ActionResult<ManagerDto>> GetManagerById(string id)
        {
            var manager = await _administrativeService.GetManagerByIdAsync(id);
            if (manager == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy tài khoản Manager với mã '{id}'."
                });
            }

            return Ok(manager);
        }

        /// <summary>
        /// Tạo mới tài khoản Manager (Role = "1", IsActive = true)
        /// Mật khẩu được mã hóa tự động bằng BCrypt
        /// Chỉ SA (Role = "2") mới có quyền gọi API này
        /// </summary>
        [HttpPost("managers")]
        [Idempotent]
        public async Task<ActionResult<ManagerDto>> CreateManager([FromBody] CreateManagerDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdManager = await _administrativeService.CreateManagerAsync(createDto);
                return CreatedAtAction(
                    nameof(GetManagerById),
                    new { id = createdManager.OrgId },
                    createdManager);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi tạo mới tài khoản Manager");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Đã xảy ra lỗi trong quá trình tạo tài khoản Manager."
                });
            }
        }

        /// <summary>
        /// Xóa (vô hiệu hóa - IsActive = false) tài khoản Manager (Role = "1")
        /// Chỉ SA (Role = "2") mới có quyền gọi API này
        /// </summary>
        [HttpDelete("managers/{id}")]
        public async Task<IActionResult> DeactivateManager(string id)
        {
            try
            {
                var deactivated = await _administrativeService.DeactivateManagerAsync(id);
                if (!deactivated)
                {
                    return NotFound(new
                    {
                        message = $"Không tìm thấy tài khoản Manager với mã '{id}'."
                    });
                }

                return Ok(new
                {
                    message = $"Đã vô hiệu hóa tài khoản Manager '{id}' thành công."
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi vô hiệu hóa tài khoản Manager: {OrgId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Đã xảy ra lỗi trong quá trình vô hiệu hóa tài khoản Manager."
                });
            }
        }
    }
}
