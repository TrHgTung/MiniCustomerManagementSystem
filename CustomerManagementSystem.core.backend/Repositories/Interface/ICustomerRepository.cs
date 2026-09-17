using CustomerManagementSystem.core.backend.Entities.Models;

namespace CustomerManagementSystem.core.backend.Repositories.Interface
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(string customerId);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<bool> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(string customerId);
        Task<bool> ExistsByIdAsync(string customerId);
        Task<bool> ExistsByEmailAsync(string email, string? excludeCustomerId = null);
        Task<IEnumerable<Customer>> GetPendingCustomersAsync();
        Task<IEnumerable<Customer>> GetCustomersByDateRangeAsync(DateTime fromDate, DateTime toDate); //phục vụ cho mục đích xuất excel theo chunk batch
        Task<IEnumerable<Customer>> SearchCustomersAsync(string keyword);
        Task<IEnumerable<Customer>> FilterCustomersAsync(string? address, int? birthYear);
    }
}