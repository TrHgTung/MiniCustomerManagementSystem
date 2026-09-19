using CustomerManagementSystem.core.backend.Data.DTO.Customer;
using CustomerManagementSystem.core.backend.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CustomerManagementSystem.core.backend.Controllers.Customer
{
    /// <summary>
    /// Form Controller: api/v1/customer/form
    /// Tiếp nhận thông tin biểu mẫu tư vấn do Khách hàng gửi từ trang chủ / client
    /// Khách hàng không cần xác thực (AllowAnonymous)
    /// </summary>
    [EnableRateLimiting("Customer")]
    [ApiController]
    [Route("customer/form")]
    [AllowAnonymous]
    public class FormController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<FormController> _logger;

        public FormController(ICustomerService customerService, ILogger<FormController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        /// <summary>
        /// Khách hàng gửi thông tin biểu mẫu tư vấn từ trang chủ (POST api/v1/customer/form)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitForm([FromBody] CustomerFormDto formDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var customer = await _customerService.SubmitConsultationFormAsync(formDto);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = "Tiếp nhận thông tin tư vấn thành công. Email xác nhận đã được gửi đến bạn.",
                    data = customer
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Yêu cầu gửi biểu mẫu không hợp lệ: {Message}", ex.Message);
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình tiếp nhận biểu mẫu tư vấn.");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Đã xảy ra lỗi trong quá trình xử lý biểu mẫu. Vui lòng thử lại sau."
                });
            }
        }
    }
}
