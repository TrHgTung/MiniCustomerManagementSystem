using CustomerManagementSystem.core.backend.Data.DTO.Customer;

namespace CustomerManagementSystem.core.backend.Services.Interface
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto?> GetCustomerByIdAsync(string customerId);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createDto);
        Task<bool> UpdateCustomerAsync(string customerId, UpdateCustomerDto updateDto);
        Task<bool> DeleteCustomerAsync(string customerId);
    }
}
