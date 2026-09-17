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
        /// lấy danh sách KH đang chờ duyệt (isActive = false)
        /// </summary>
        public async Task<IEnumerable<Customer>> GetPendingCustomersAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .Where(c => !c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}