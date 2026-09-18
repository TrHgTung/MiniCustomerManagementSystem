using System.Security.Claims;
using CustomerManagementSystem.core.backend.Data.DTO.Customer;
using CustomerManagementSystem.core.backend.Helpers;
using CustomerManagementSystem.core.backend.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagementSystem.core.backend.Controllers.Admin
{
    /// <summary>
    /// Export Data Controller: api/v1/admin/export
    /// Xuất dữ liệu báo cáo ra bảng tính Excel
    /// Cho phép cả SA (Role = "2") và Manager (Role = "1") truy cập (Policy = ManagerOrAdmin)
    /// </summary>
    [Authorize(Policy = "ManagerOrAdmin")]
    [ApiController]
    [Route("admin/export")]
    public class ExportDataController : ControllerBase
    {
        private readonly IExcelExportService _excelExportService;
        private readonly ILogger<ExportDataController> _logger;

        public ExportDataController(
            IExcelExportService excelExportService,
            ILogger<ExportDataController> logger)
        {
            _excelExportService = excelExportService;
            _logger = logger;
        }

        /// <summary>
        /// Xuất danh sách khách hàng ra file Excel theo khoảng thời gian (CreatedAt)
        /// SA: xuất toàn bộ khách hàng trong khoảng thời gian
        /// Manager: chỉ xuất các khách hàng có IsActive = true trong khoảng thời gian
        /// </summary>
        /// <param name="filter">Khoảng thời gian FromDate và ToDate</param>
        /// <returns>File Excel .xlsx tải về</returns>
        [HttpGet("customers")]
        public async Task<IActionResult> ExportCustomers([FromQuery] ExportCustomerFilterDto filter)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (filter.FromDate > filter.ToDate)
            {
                return BadRequest(new
                {
                    message = "Ngày bắt đầu (FromDate) không được lớn hơn ngày kết thúc (ToDate)."
                });
            }

            try
            {
                var userRole = User.GetCurrentUserRole();

                // Nếu toDate chỉ có phần ngày (00:00:00), mở rộng đến cuối ngày 23:59:59.9999999 để lấy đủ dữ liệu trong ngày
                var toDate = filter.ToDate.TimeOfDay == TimeSpan.Zero
                    ? filter.ToDate.Date.AddDays(1).AddTicks(-1)
                    : filter.ToDate;

                var fileBytes = await _excelExportService.ExportCustomersToExcelAsync(filter.FromDate, toDate, userRole);

                string fileName = $"DanhSachKH_{filter.FromDate:yyyyMMdd}_{filter.ToDate:yyyyMMdd}_{Guid.NewGuid().ToString().Substring(0, 6)}.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình xuất dữ liệu Excel khách hàng.");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Đã xảy ra lỗi trong quá trình xuất dữ liệu Excel."
                });
            }
        }
    }
}
