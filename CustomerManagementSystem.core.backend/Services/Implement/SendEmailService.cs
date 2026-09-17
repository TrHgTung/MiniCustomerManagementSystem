using System.Net;
using System.Net.Mail;
using CustomerManagementSystem.core.backend.Configurations;
using CustomerManagementSystem.core.backend.Services.Interface;
using Microsoft.Extensions.Options;

namespace CustomerManagementSystem.core.backend.Services.Implement
{
    /// <summary>
    /// Service triển khai logic gửi email qua giao thức SMTP dựa trên cấu hình hệ thống
    /// </summary>
    public class SendEmailService : ISendEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<SendEmailService> _logger;

        public SendEmailService(IOptions<EmailSettings> emailSettings, ILogger<SendEmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Gửi email chung theo tiêu đề và nội dung
        /// </summary>
        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning("Địa chỉ email người nhận trống. Hủy tác vụ gửi email.");
                return;
            }

            try
            {
                // Kiểm tra xem đã cấu hình SMTP server hay chưa
                if (string.IsNullOrWhiteSpace(_emailSettings.SmtpServer) || string.IsNullOrWhiteSpace(_emailSettings.SenderEmail))
                {
                    _logger.LogWarning("Cấu hình EmailSettings chưa đầy đủ (SmtpServer/SenderEmail trống). Ghi nhận nội dung email vào log thay thế: Đến: {To}, Tiêu đề: {Subject}", toEmail, subject);
                    return;
                }

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mailMessage.To.Add(toEmail);

                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    UseDefaultCredentials = false
                };

                if (!string.IsNullOrWhiteSpace(_emailSettings.Username) && !string.IsNullOrWhiteSpace(_emailSettings.Password))
                {
                    smtpClient.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                }

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Gửi email thành công tới: {ToEmail} với tiêu đề: '{Subject}'", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi gửi email tới {ToEmail}. Tiêu đề: '{Subject}'", toEmail, subject);
            }
        }

        /// <summary>
        /// Gửi email tự động xác nhận phản hồi sau khi khách hàng gửi biểu mẫu tư vấn
        /// </summary>
        public async Task SendConsultationConfirmationEmailAsync(string toEmail, string customerName)
        {
            string subject = "[CSKH] Xác nhận tiếp nhận thông tin yêu cầu tư vấn";

            string body = $@"<p>Xin chào <strong>{WebUtility.HtmlEncode(customerName)}</strong>,</p><br/>
                            <p>Chúng tôi đã tiếp nhận thông tin của bạn, và chúng tôi sẽ liên hệ bạn qua thông tin được lưu trữ để tư vấn thêm.</p><br/>
                            <p>Trân trọng,<br/>Đội ngũ Chăm sóc Khách hàng</p>";

            await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }
    }
}
