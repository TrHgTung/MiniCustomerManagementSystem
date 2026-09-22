using CustomerManagementSystem.core.backend.Entities.AppDataContext;
using CustomerManagementSystem.core.backend.Entities.Models;
using CustomerManagementSystem.core.backend.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementSystem.core.backend.Services.Implement
{
    /// <summary>
    /// Triển khai IIdempotencyService: thao tác với bảng Idempotencies trong database.
    /// </summary>
    public class IdempotencyService : IIdempotencyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IdempotencyService> _logger;

        public IdempotencyService(ApplicationDbContext context, ILogger<IdempotencyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Idempotency?> GetAsync(string key)
        {
            return await _context.Idempotencies
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Key == key && x.ExpiresAt > DateTime.UtcNow);
        }

        /// <inheritdoc />
        public async Task<bool> TryCreatePendingAsync(string key, string requestPath, TimeSpan expireDuration)
        {
            var record = new Idempotency
            {
                Key = key,
                RequestPath = requestPath,
                StatusCode = 0, // pending: đang xử lý
                ResponseBody = string.Empty,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(expireDuration)
            };

            try
            {
                _context.Idempotencies.Add(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                // Unique constraint violation: key đã tồn tại (race condition)
                _logger.LogWarning("Idempotency key '{Key}' đã tồn tại (concurrent request).", key);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task CompleteAsync(string key, int statusCode, string responseBody)
        {
            var record = await _context.Idempotencies
                .FirstOrDefaultAsync(x => x.Key == key);

            if (record != null)
            {
                record.StatusCode = statusCode;
                record.ResponseBody = responseBody;
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string key)
        {
            var record = await _context.Idempotencies
                .FirstOrDefaultAsync(x => x.Key == key);

            if (record != null)
            {
                _context.Idempotencies.Remove(record);
                await _context.SaveChangesAsync();
            }
        }
    }
}
