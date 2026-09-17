using ClosedXML.Excel;
using CustomerManagementSystem.core.backend.Repositories.Interface;
using CustomerManagementSystem.core.backend.Services.Interface;

namespace CustomerManagementSystem.core.backend.Services.Implement
{
    /// <summary>
    /// Service thực hiện xuất dữ liệu khách hàng ra file Excel (.xlsx)
    /// </summary>
    public class ExportExcelService : IExcelExportService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<ExportExcelService> _logger;

        public ExportExcelService(ICustomerRepository customerRepository, ILogger<ExportExcelService> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        /// <summary>
        /// Xuất danh sách khách hàng trong khoảng thời gian (CreatedAt) ra file Excel byte[]
        /// SA (Role="2"): xuất toàn bộ
        /// Manager (Role="1"): chỉ xuất các KH có IsActive = true
        /// </summary>
        public async Task<byte[]> ExportCustomersToExcelAsync(DateTime fromDate, DateTime toDate, string userRole)
        {
            var customers = await _customerRepository.GetCustomersByDateRangeAsync(fromDate, toDate);

            // accoun Manager (Role = "1") chỉ được xem/xuất khách hàng có IsActive = true
            if (userRole == "1")
            {
                customers = customers.Where(c => c.IsActive);
            }

            var customerList = customers.ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Danh Sách Khách Hàng");

            // Header titles
            string[] headers =
            {
                "STT",
                "Mã KH",
                "Họ và tên",
                "Email",
                "Số điện thoại",
                "Ngày sinh",
                "Nơi sinh sống",
                "Trạng thái",
                "Thời gian tạo"
            };

            // Tiêu đề bảng
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(13, 110, 253); // Màu xanh dương chuyên nghiệp
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }

            worksheet.Row(1).Height = 25;

            // Điền dữ liệu khách hàng
            int row = 2;
            int stt = 1;
            foreach (var customer in customerList)
            {
                worksheet.Cell(row, 1).Value = stt++;
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 2).Value = customer.CustomerId;
                worksheet.Cell(row, 3).Value = customer.CustomerName;
                worksheet.Cell(row, 4).Value = customer.CustomerEmail;
                worksheet.Cell(row, 5).Value = customer.CustomerPhone;
                worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 6).Value = customer.CustomerBirth.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 7).Value = customer.CustomerAddress;

                worksheet.Cell(row, 8).Value = customer.IsActive ? "Hoạt động" : "Chờ duyệt";
                worksheet.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                if (!customer.IsActive)
                {
                    worksheet.Cell(row, 8).Style.Font.FontColor = XLColor.FromArgb(220, 53, 69); // Màu đỏ cảnh báo
                }
                else
                {
                    worksheet.Cell(row, 8).Style.Font.FontColor = XLColor.FromArgb(25, 135, 84); // Màu xanh lá
                }

                worksheet.Cell(row, 9).Value = customer.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss");
                worksheet.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
            }

            // Kẻ viền bảng cho toàn bộ vùng dữ liệu
            var dataRange = worksheet.Range(1, 1, Math.Max(row - 1, 1), headers.Length);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorderColor = XLColor.LightGray;
            dataRange.Style.Border.InsideBorderColor = XLColor.LightGray;

            // Tự động căn chỉnh độ rộng cột
            worksheet.Columns().AdjustToContents();

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);

            _logger.LogInformation("Đã xuất báo cáo Excel thành công cho {Count} khách hàng (từ {FromDate} đến {ToDate}) bởi vai trò {Role}.",
                customerList.Count, fromDate, toDate, userRole);

            return memoryStream.ToArray();
        }
    }
}
