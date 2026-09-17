using CustomerManagementSystem.core.backend.Data.DTO.Customer;
using CustomerManagementSystem.core.backend.Helpers;
using CustomerManagementSystem.core.backend.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagementSystem.core.backend.Controllers.Admin
{
    /// <summary>
    /// Filter Customer Controller: api/v1/admin/customers/filter
    /// Lọc KH theo Nơi sinh sống (CustomerAddress - Tỉnh/Thành) và/hoặc Năm sinh (CustomerBirth)
    /// SA (Role="2"): lọc trên toàn bộ KH
    /// Manager (Role="1"): chỉ lọc trên các KH có IsActive = true
    /// </summary>
    [Authorize(Policy = "ManagerOrAdmin")]
    [ApiController]
    [Route("admin/customers/filter")]
    public class FilterCustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<FilterCustomerController> _logger;

        public FilterCustomerController(ICustomerService customerService, ILogger<FilterCustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        /// <summary>
        /// Lọc KH theo Nơi sinh sống (address) và/hoặc Năm sinh (birthYear)
        /// Ít nhất một trong hai tham số phải được cung cấp
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> Filter([FromQuery] string? address, [FromQuery] int? birthYear)
        {
            if (string.IsNullOrWhiteSpace(address) && !birthYear.HasValue)
            {
                return BadRequest(new
                {
                    message = "Cần cung cấp ít nhất một điều kiện lọc: Nơi sinh sống (address) hoặc Năm sinh (birthYear)."
                });
            }

            try
            {
                var userRole = User.GetCurrentUserRole();
                var customers = await _customerService.FilterCustomersAsync(address, birthYear, userRole);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi lọc KH với address: {Address}, birthYear: {BirthYear}", address, birthYear);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Đã xảy ra lỗi khi lọc data"
                });
            }
        }
    }
}
