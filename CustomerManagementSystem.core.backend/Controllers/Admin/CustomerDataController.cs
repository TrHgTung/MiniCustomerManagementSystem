using CustomerManagementSystem.core.backend.Data.DTO.Customer;
using CustomerManagementSystem.core.backend.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagementSystem.core.backend.Controllers.Admin
{
    /// <summary>
    /// Customer Data Controller: api/v1/admin/customers
    /// Hiển thị danh sách khách hàng trong trang admin
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
        /// lấy toàn bộ danh sách KH
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        /// <summary>
        /// thông tin chi tiết 1 KH
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

            return Ok(customer);
        }

        /// <summary>
        /// tạo mới 1 KH
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
                var createdCustomer = await _customerService.CreateCustomerAsync(createDto);
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
                var updated = await _customerService.UpdateCustomerAsync(id, updateDto);
                if (!updated)
                {
                    return NotFound(new
                    {
                        message = $"Không tìm thấy mã KH '{id}' "
                    });
                }

                return Ok(new
                {
                    message = "Cập nhật thành công"
                });
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
        /// xóa KH (Chỉ dành cho Quản trị viên - Role == "2")
        /// </summary>
        [Authorize(Roles = "2")]
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
    }
}
