using System.Security.Claims;
using CustomerManagementSystem.core.backend.Data.DTO.Customer;
using CustomerManagementSystem.core.backend.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagementSystem.core.backend.Controllers.Admin
{
    /// <summary>
    /// Customer Data Controller: api/v1/admin/customers
    /// Hiển thị danh sách khách hàng trong trang admin
    /// 
    /// SA (Role="2"): thêm/sửa/xóa trực tiếp, isActive = true ngay
    /// Manager (Role="1"): thêm/sửa được nhưng isActive = false, chờ SA duyệt approve
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("admin/customers")]
    public class CustomerDataController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerDataController> _logger; // co san trong using Microsoft.Extensions.Logging -> log console

        public CustomerDataController(ICustomerService customerService, ILogger<CustomerDataController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        /// <summary>
        /// lấy danh sách KH
        /// SA (Role = "2"): xem toàn bộ KH
        /// Manager (Role = "1"): chỉ xem các KH có isActive = true
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
        {
            var userRole = GetCurrentUserRole();
            var customers = await _customerService.GetAllCustomersAsync();

            if (userRole == "1")
            {
                customers = customers.Where(c => c.IsActive);
            }

            return Ok(customers);
        }

        /// <summary>
        /// thông tin chi tiết 1 KH
        /// SA (Role = "2"): xem được mọi KH
        /// Manager (Role = "1"): chỉ xem được KH có isActive = true
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetById(string id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy KH với mã '{id}'."
                });
            }

            var userRole = GetCurrentUserRole();
            if (userRole == "1" && !customer.IsActive)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy KH với mã '{id}'."
                });
            }

            return Ok(customer);
        }

        /// <summary>
        /// tạo mới 1 KH
        /// SA -> isActive = true ngay, Manager -> isActive = false (chờ duyệt)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userRole = GetCurrentUserRole();
                var createdCustomer = await _customerService.CreateCustomerAsync(createDto, userRole);
                return CreatedAtAction(nameof(GetById), new { id = createdCustomer.CustomerId }, createdCustomer);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi tạo mới KH");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Đã xảy ra lỗi"
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin KH
        /// SA -> giữ nguyên isActive, Manager -> isActive = false (chờ duyệt lại)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateCustomerDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userRole = GetCurrentUserRole();
                var updated = await _customerService.UpdateCustomerAsync(id, updateDto, userRole);
                if (!updated)
                {
                    return NotFound(new {
                        message = $"Không tìm thấy mã KH '{id}' "
                    });
                }

                return Ok(new {
                    message = userRole == "2"
                        ? "Cập nhật thành công"
                        : "Cập nhật thành công, đang chờ SA duyệt"
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi cập nhật KH: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new {
                    message = "Đã xảy ra lỗi"
                });
            }
        }

        /// <summary>
        /// xóa KH
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var deleted = await _customerService.DeleteCustomerAsync(id);
                if (!deleted)
                {
                    return NotFound(new {
                        message = $"Không tìm thấy KH '{id}'"
                    });
                }

                return Ok(new {
                    message = "Xóa thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa KH: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new {
                    message = "Đã xảy ra lỗi"
                });
            }
        }

        /// <summary>
        /// SA duyệt approve cho KH đang chờ (isActive: false -> true)
        /// Chỉ dành cho SA (Role = "2")
        /// </summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> Approve(string id)
        {
            try
            {
                var approved = await _customerService.ApproveCustomerAsync(id);
                if (!approved)
                {
                    return NotFound(new {
                        message = $"Không tìm thấy KH '{id}'"
                    });
                }

                return Ok(new {
                    message = $"Đã duyệt approve KH '{id}' thành công"
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi duyệt KH: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new {
                    message = "Đã xảy ra lỗi"
                });
            }
        }

        /// <summary>
        /// lấy danh sách KH đang chờ duyệt (isActive = false)
        /// Chỉ dành cho SA (Role = "2")
        /// </summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetPending()
        {
            var customers = await _customerService.GetPendingCustomersAsync();
            return Ok(customers);
        }

        /// <summary>
        /// helper: lấy Role của user hiện tại từ JWT Claims
        /// </summary>
        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value
                ?? User.FindFirst("role")?.Value
                ?? "1"; // mặc định là Manager nếu không xác định được
        }
    }
}
