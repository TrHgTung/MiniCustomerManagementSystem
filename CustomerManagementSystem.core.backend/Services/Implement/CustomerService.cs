using CustomerManagementSystem.core.backend.Data.DTO.Customer;
using CustomerManagementSystem.core.backend.Entities.Models;
using CustomerManagementSystem.core.backend.Repositories.Interface;
using CustomerManagementSystem.core.backend.Services.Interface;

namespace CustomerManagementSystem.core.backend.Services.Implement
{
    /// <summary>
    /// Service: dùng để "thao tác nghiệp vụ", tách hành vi nghiệp vụ ra khỏi controller
    /// 
    /// Quy tắc phân quyền với dữ liệu KH:
    /// - SA (Role="2"): thêm/sửa KH → isActive = true ngay lập tức
    /// - Manager (Role="1"): thêm/sửa KH → isActive = false (chờ SA duyệt approve)
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
        /// - SA (Role="2"): isActive = true (duyệt ngay)
        /// - Manager (Role="1"): isActive = false (chờ SA duyệt)
        /// </summary>
        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createDto, string userRole)
        {
            // check if Email already exists
            if (await _customerRepository.ExistsByEmailAsync(createDto.CustomerEmail))
            {
                throw new InvalidOperationException($"Email '{createDto.CustomerEmail}' đã được sử dụng.");
            }

            // SA (Role="2") -> active ngay, Manager (Role="1") -> chờ duyệt
            bool isActive = true;
            if (userRole == "1") // role: Manager account
            {
                isActive = false;
            }

            var customer = new Customer
            {
                CustomerId = Guid.NewGuid().ToString(),
                CustomerName = createDto.CustomerName.Trim(),
                CustomerEmail = createDto.CustomerEmail.Trim().ToLowerInvariant(),
                CustomerPhone = createDto.CustomerPhone.Trim(),
                CustomerBirth = createDto.CustomerBirth,
                CustomerAddress = createDto.CustomerAddress.Trim(),
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow
            };

            var createdCustomer = await _customerRepository.CreateCustomerAsync(customer);

            if (isActive)
            {
                _logger.LogInformation("SA tạo KH trực tiếp: {CustomerId}", createdCustomer.CustomerId);
            }
            else
            {
                _logger.LogInformation("Manager tạo KH (chờ duyệt): {CustomerId}", createdCustomer.CustomerId);
            }

            return MapToDto(createdCustomer);
        }

        /// <summary>
        /// cập nhật KH
        /// - SA (Role="2"): cập nhật và giữ nguyên trạng thái isActive hiện tại
        /// - Manager (Role="1"): cập nhật nhưng isActive = false (cần SA duyệt lại)
        /// </summary>
        public async Task<bool> UpdateCustomerAsync(string customerId, UpdateCustomerDto updateDto, string userRole)
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

            // SA giữ nguyên isActive, Manager thì set false (chờ duyệt lại)
            if (userRole == "2")
            {
                existingCustomer.IsActive = updateDto.IsActive;
            }
            else
            {
                existingCustomer.IsActive = false;
            }

            var result = await _customerRepository.UpdateCustomerAsync(existingCustomer);
            if (result)
            {
                if (userRole == "2")
                {
                    _logger.LogInformation("SA cập nhật KH: {CustomerId}", customerId);
                }
                else
                {
                    _logger.LogInformation("Manager cập nhật KH (chờ duyệt lại): {CustomerId}", customerId);
                }
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
        /// SA duyệt approve cho KH đang chờ (isActive: false -> true)
        /// </summary>
        public async Task<bool> ApproveCustomerAsync(string customerId)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                return false;
            }

            if (customer.IsActive)
            {
                throw new InvalidOperationException("Yêu cầu này đã được duyệt trước đó rồi");
            }

            customer.IsActive = true;
            var result = await _customerRepository.UpdateCustomerAsync(customer);
            if (result)
            {
                _logger.LogInformation("SA đã duyệt approve cho KH: {CustomerId}", customerId);
            }

            return result;
        }

        /// <summary>
        /// lấy danh sách KH đang chờ duyệt (isActive = false)
        /// </summary>
        public async Task<IEnumerable<CustomerDto>> GetPendingCustomersAsync()
        {
            var customers = await _customerRepository.GetPendingCustomersAsync();
            return customers.Select(MapToDto);
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
