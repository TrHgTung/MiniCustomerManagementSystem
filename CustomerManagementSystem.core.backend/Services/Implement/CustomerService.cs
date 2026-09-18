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
        private readonly ISendEmailService _sendEmailService;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(ICustomerRepository customerRepository, ISendEmailService sendEmailService, ILogger<CustomerService> logger)
        {
            _customerRepository = customerRepository;
            _sendEmailService = sendEmailService;
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
        /// xóa KH:
        /// - SA (Role="2"): xóa vĩnh viễn khỏi hệ thống
        /// - Manager (Role="1"): yêu cầu xóa (đánh dấu DeletedAt, chờ SA duyệt)
        /// </summary>
        public async Task<bool> DeleteCustomerAsync(string customerId, string userRole)
        {
            if (userRole == "2")
            {
                var result = await _customerRepository.DeleteCustomerAsync(customerId);
                if (result)
                {
                    _logger.LogInformation("SA đã xóa vĩnh viễn KH: {CustomerId}", customerId);
                }
                return result;
            }
            else
            {
                var result = await _customerRepository.SoftDeleteCustomerAsync(customerId);
                if (result)
                {
                    _logger.LogInformation("Manager đã yêu cầu xóa KH (chờ SA duyệt): {CustomerId}", customerId);
                }
                return result;
            }
        }

        /// <summary>
        /// lấy danh sách KH đang chờ xóa (DeletedAt != null) - Dành riêng cho SA
        /// </summary>
        public async Task<IEnumerable<CustomerDto>> GetPendingDeletionCustomersAsync()
        {
            var customers = await _customerRepository.GetPendingDeletionCustomersAsync();
            return customers.Select(MapToDto);
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
        /// Tiếp nhận và lưu thông tin biểu mẫu tư vấn do khách hàng gửi từ Client/Trang chủ,
        /// đồng thời gửi email phản hồi xác nhận tự động.
        /// </summary>
        public async Task<CustomerDto> SubmitConsultationFormAsync(CustomerFormDto formDto)
        {
            // Kiểm tra email đã tồn tại hay chưa
            if (await _customerRepository.ExistsByEmailAsync(formDto.CustomerEmail))
            {
                throw new InvalidOperationException($"Email '{formDto.CustomerEmail}' đã được đăng ký trong hệ thống.");
            }

            var customer = new Customer
            {
                CustomerId = Guid.NewGuid().ToString(),
                CustomerName = formDto.CustomerName.Trim(),
                CustomerEmail = formDto.CustomerEmail.Trim().ToLowerInvariant(),
                CustomerPhone = formDto.CustomerPhone.Trim(),
                CustomerBirth = formDto.CustomerBirth,
                CustomerAddress = formDto.CustomerAddress.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdCustomer = await _customerRepository.CreateCustomerAsync(customer);
            _logger.LogInformation("Đã tiếp nhận và lưu thông tin form tư vấn cho KH: {CustomerId} ({Email})", createdCustomer.CustomerId, createdCustomer.CustomerEmail);

            // Gửi email phản hồi tự động đến khách hàng
            try
            {
                await _sendEmailService.SendConsultationConfirmationEmailAsync(createdCustomer.CustomerEmail, createdCustomer.CustomerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi gửi email xác nhận cho KH: {Email}. Dữ liệu KH đã được lưu an toàn.", createdCustomer.CustomerEmail);
            }

            return MapToDto(createdCustomer);
        }

        /// <summary>
        /// tìm kiếm KH theo từ khóa trùng khớp trong: CustomerId, CustomerName, CustomerEmail, CustomerPhone
        /// Manager (Role="1"): chỉ trả về KH có IsActive = true
        /// </summary>
        public async Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string keyword, string userRole)
        {
            var customers = await _customerRepository.SearchCustomersAsync(keyword);

            if (userRole == "1")
            {
                customers = customers.Where(c => c.IsActive);
            }

            return customers.Select(MapToDto);
        }

        /// <summary>
        /// lọc KH theo Nơi sinh sống (CustomerAddress) và/hoặc Năm sinh (lấy Year từ CustomerBirth)
        /// Manager (Role="1"): chỉ trả về KH có IsActive = true
        /// </summary>
        public async Task<IEnumerable<CustomerDto>> FilterCustomersAsync(string? address, int? birthYear, string userRole)
        {
            var customers = await _customerRepository.FilterCustomersAsync(address, birthYear);

            if (userRole == "1")
            {
                customers = customers.Where(c => c.IsActive);
            }

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
