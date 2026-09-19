using CustomerManagementSystem.core.backend.Entities.Models;

namespace CustomerManagementSystem.core.backend.Services.Interface
{
    /// <summary>
    /// Service quản lý Idempotency: kiểm tra, tạo pending, cập nhật kết quả, và xóa key.
    /// </summary>
    public interface IIdempotencyService
    {
        /// <summary>
        /// Lấy bản ghi Idempotency theo key (chưa hết hạn).
        /// </summary>
        Task<Idempotency?> GetAsync(string key);

        /// <summary>
        /// Tạo bản ghi pending (StatusCode = 0) để đánh dấu request đang được xử lý.
        /// Trả về true nếu tạo thành công, false nếu key đã tồn tại (race condition).
        /// </summary>
        Task<bool> TryCreatePendingAsync(string key, string requestPath, TimeSpan expireDuration);

        /// <summary>
        /// Cập nhật bản ghi khi action đã thực thi xong: ghi nhận StatusCode và ResponseBody.
        /// </summary>
        Task CompleteAsync(string key, int statusCode, string responseBody);

        /// <summary>
        /// Xóa bản ghi Idempotency (khi xảy ra lỗi, để client có thể retry an toàn).
        /// </summary>
        Task RemoveAsync(string key);
    }
}
