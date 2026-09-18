using CustomerManagementSystem.core.backend.Entities.AppDataContext;
using CustomerManagementSystem.core.backend.Entities.Models;
using CustomerManagementSystem.core.backend.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementSystem.core.backend.Repositories.Implement
{
    /// <summary>
    /// Repository: dùng để "thao tác trực tiếp với Database", tách hành vi truy vấn ra khỏi business logcial
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// lấy all KH
        /// </summary>
        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .Where(c => c.DeletedAt == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// lấy thông tin KH theo customerId
        /// </summary>
        public async Task<Customer?> GetCustomerByIdAsync(string customerId)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        /// <summary>
        /// thêm mới KH
        /// </summary>
        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        /// <summary>
        /// cập nhật KH
        /// </summary>
        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            var result = await _context.SaveChangesAsync();
            if(result > 0) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// xóa KH
        /// </summary>
        public async Task<bool> DeleteCustomerAsync(string customerId)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null) {
                return false;
            }
            _context.Customers.Remove(customer);

            var result = await _context.SaveChangesAsync();
            if(result > 0) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// yêu cầu xóa KH (Manager): đánh dấu DeletedAt, tạm ngừng isActive
        /// </summary>
        public async Task<bool> SoftDeleteCustomerAsync(string customerId)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null) {
                return false;
            }

            customer.DeletedAt = DateTime.UtcNow;
            customer.IsActive = false;

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        /// <summary>
        /// kiểm tra xem có customerId nào trùng lặp không
        /// </summary>
        public async Task<bool> ExistsByIdAsync(string customerId)
        {
            return await _context.Customers
                .AnyAsync(c => c.CustomerId == customerId);
        }

        /// <summary>
        /// kiểm tra xem có email nào trùng lặp không
        /// </summary>
        public async Task<bool> ExistsByEmailAsync(string email, string? excludeCustomerId = null)
        {
            var query = _context.Customers.AsNoTracking().Where(c => c.CustomerEmail == email);
            if (!string.IsNullOrEmpty(excludeCustomerId))
            {
                query = query.Where(c => c.CustomerId != excludeCustomerId);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// lấy danh sách KH đang chờ duyệt tạo mới / sửa (isActive = false và chưa bị đánh dấu xóa)
        /// </summary>
        public async Task<IEnumerable<Customer>> GetPendingCustomersAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .Where(c => !c.IsActive && c.DeletedAt == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// lấy danh sách KH đang chờ xóa (DeletedAt != null)
        /// Chỉ dành cho SA (Role = "2")
        /// </summary>
        public async Task<IEnumerable<Customer>> GetPendingDeletionCustomersAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .Where(c => c.DeletedAt != null)
                .OrderByDescending(c => c.DeletedAt)
                .ToListAsync();
        }

        /// <summary>
        /// lấy danh sách KH được tạo trong khoảng thời gian (CreatedAt)
        /// phục vụ cho mục đích xuất excel theo chunk batch
        /// </summary>
        public async Task<IEnumerable<Customer>> GetCustomersByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Customers
                .AsNoTracking()
                .Where(c => c.DeletedAt == null && c.CreatedAt >= fromDate && c.CreatedAt <= toDate)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// tìm kiếm KH theo ký tự trùng khớp trong các cột 
        /// CustomerId, CustomerName, CustomerEmail, CustomerPhone (chưa bị đánh dấu xóa)
        /// </summary>
        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string keyword)
        {
            var lowerKeyword = keyword.Trim().ToLowerInvariant();

            return await _context.Customers
                .AsNoTracking()
                .Where(c => c.DeletedAt == null && (
                    c.CustomerId.ToLower().Contains(lowerKeyword) ||
                    c.CustomerName.ToLower().Contains(lowerKeyword) ||
                    c.CustomerEmail.ToLower().Contains(lowerKeyword) ||
                    c.CustomerPhone.ToLower().Contains(lowerKeyword)
                ))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// lọc KH theo Nơi sinh sống (CustomerAddress)
        /// và/hoặc Năm sinh (trích Year từ trường CustomerBirth kiểu DateTime)
        /// </summary>
        public async Task<IEnumerable<Customer>> FilterCustomersAsync(string? address, int? birthYear)
        {
            var query = _context.Customers.AsNoTracking().Where(c => c.DeletedAt == null).AsQueryable();

            if (!string.IsNullOrWhiteSpace(address))
            {
                var lowerAddress = address.Trim().ToLowerInvariant();
                query = query.Where(c => c.CustomerAddress.ToLower().Contains(lowerAddress));
            }

            if (birthYear.HasValue)
            {
                query = query.Where(c => c.CustomerBirth.Year == birthYear.Value);
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}