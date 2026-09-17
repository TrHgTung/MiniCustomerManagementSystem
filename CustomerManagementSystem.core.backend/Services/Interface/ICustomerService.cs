using CustomerManagementSystem.core.backend.Data.DTO.Customer;

namespace CustomerManagementSystem.core.backend.Services.Interface
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto?> GetCustomerByIdAsync(string customerId);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createDto, string userRole);
        Task<bool> UpdateCustomerAsync(string customerId, UpdateCustomerDto updateDto, string userRole);
        Task<bool> DeleteCustomerAsync(string customerId);
        Task<bool> ApproveCustomerAsync(string customerId);
        Task<IEnumerable<CustomerDto>> GetPendingCustomersAsync();
    }
}
