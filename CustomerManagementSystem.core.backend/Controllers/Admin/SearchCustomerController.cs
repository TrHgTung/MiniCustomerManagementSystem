using CustomerManagementSystem.core.backend.Data.DTO.Customer;
using CustomerManagementSystem.core.backend.Helpers;
using CustomerManagementSystem.core.backend.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagementSystem.core.backend.Controllers.Admin
{
    /// <summary>
    /// Search Customer Controller: api/v1/admin/customers/search
    /// Tìm kiếm KH theo ký tự trùng khớp trong: CustomerId, CustomerName, CustomerEmail, CustomerPhone
    /// SA (Role="2"): tìm trên toàn bộ KH
    /// Manager (Role="1"): chỉ tìm trên các KH có IsActive = true
    /// </summary>
    [Authorize(Policy = "ManagerOrAdmin")]
    [ApiController]
    [Route("admin/customers/search")]
    public class SearchCustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<SearchCustomerController> _logger;

        public SearchCustomerController(ICustomerService customerService, ILogger<SearchCustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        /// <summary>
        /// Tìm kiếm KH theo từ khóa (keyword) trùng khớp trong: Mã KH, Họ tên, Email, SĐT
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new
                {
                    message = "Search keyword đang null"
                });
            }

            try
            {
                var userRole = User.GetCurrentUserRole();
                var customers = await _customerService.SearchCustomersAsync(keyword, userRole);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi tìm kiếm KH với từ khóa: {Keyword}", keyword);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Đã xảy ra lỗi khi search"
                });
            }
        }
    }
}
