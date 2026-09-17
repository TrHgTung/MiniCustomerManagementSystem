namespace CustomerManagementSystem.core.backend.Services.Interface
{
    /// <summary>
    /// Service phụ trách gửi email từ hệ thống
    /// </summary>
    public interface ISendEmailService
    {
        /// <summary>
        /// Gửi email chung theo tiêu đề và nội dung
        /// </summary>
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Gửi email tự động xác nhận phản hồi sau khi khách hàng gửi biểu mẫu tư vấn
        /// </summary>
        Task SendConsultationConfirmationEmailAsync(string toEmail, string customerName);
    }
}
