using CustomerManagementSystem.core.backend.Data.DTO.Customer;
using CustomerManagementSystem.core.backend.Entities.Models;
using CustomerManagementSystem.core.backend.Repositories.Interface;
using CustomerManagementSystem.core.backend.Services.Interface;

namespace CustomerManagementSystem.core.backend.Services.Implement
{
    /// <summary>
    /// Service: dùng để "thao tác nghiệp vụ", tách hành vi nghiệp vụ ra khỏi controller
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(ICustomerRepository customerRepository, ILogger<CustomerService> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        /// <summary>
        /// lấy all KH
        /// </summary>
        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _customerRepository.GetAllCustomersAsync();
            return customers.Select(MapToDto);
        }

        /// <summary>
        /// lấy thông tin KH theo customerId
        /// </summary>
        public async Task<CustomerDto?> GetCustomerByIdAsync(string customerId)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            return customer == null ? null : MapToDto(customer);
        }

        /// <summary>
        /// thêm mới KH
        /// </summary>
        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createDto)
        {
            // check if Email already exists
            if (await _customerRepository.ExistsByEmailAsync(createDto.CustomerEmail))
            {
                throw new InvalidOperationException($"Email '{createDto.CustomerEmail}' đã được sử dụng.");
            }

            var customer = new Customer
            {
                CustomerId = Guid.NewGuid().ToString(),
                CustomerName = createDto.CustomerName.Trim(),
                CustomerEmail = createDto.CustomerEmail.Trim().ToLowerInvariant(),
                CustomerPhone = createDto.CustomerPhone.Trim(),
                CustomerBirth = createDto.CustomerBirth,
                CustomerAddress = createDto.CustomerAddress.Trim(),
                IsActive = createDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var createdCustomer = await _customerRepository.CreateCustomerAsync(customer);
            _logger.LogInformation("Tạo KH thành công: {CustomerId}", createdCustomer.CustomerId);

            return MapToDto(createdCustomer);
        }

        /// <summary>
        /// cập nhật KH
        /// </summary>
        public async Task<bool> UpdateCustomerAsync(string customerId, UpdateCustomerDto updateDto)
        {
            var existingCustomer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (existingCustomer == null)
            {
                return false;
            }

            // Business Rule: Check if Email is changed and conflicts with another customer
            if (await _customerRepository.ExistsByEmailAsync(updateDto.CustomerEmail, customerId))
            {
                throw new InvalidOperationException($"Email '{updateDto.CustomerEmail}' đã được sử dụng, hãy đổi mail khác");
            }

            existingCustomer.CustomerName = updateDto.CustomerName.Trim();
            existingCustomer.CustomerEmail = updateDto.CustomerEmail.Trim().ToLowerInvariant();
            existingCustomer.CustomerPhone = updateDto.CustomerPhone.Trim();
            existingCustomer.CustomerBirth = updateDto.CustomerBirth;
            existingCustomer.CustomerAddress = updateDto.CustomerAddress.Trim();
            existingCustomer.IsActive = updateDto.IsActive;

            var result = await _customerRepository.UpdateCustomerAsync(existingCustomer);
            if (result)
            {
                _logger.LogInformation("Cập nhật thông tin Id KH thành công: {CustomerId}", customerId);
            }

            return result;
        }

        /// <summary>
        /// xóa KH
        /// </summary>
        public async Task<bool> DeleteCustomerAsync(string customerId)
        {
            var result = await _customerRepository.DeleteCustomerAsync(customerId);
            if (result)
            {
                _logger.LogInformation("Xóa Id KH thành công: {CustomerId}", customerId);
            }
            return result;
        }

        /// <summary>
        /// map data từ entity KH sang DTO, bảo mật data trước khi trả lên cho client
        /// </summary>
        private static CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                CustomerId = customer.CustomerId,
                CustomerName = customer.CustomerName,
                CustomerEmail = customer.CustomerEmail,
                CustomerPhone = customer.CustomerPhone,
                CustomerBirth = customer.CustomerBirth,
                CustomerAddress = customer.CustomerAddress,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt
            };
        }
    }
}
