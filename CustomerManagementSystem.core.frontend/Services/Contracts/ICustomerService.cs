using CustomerManagementSystem.core.frontend.Models.Common;
using CustomerManagementSystem.core.frontend.Models.Customer;

namespace CustomerManagementSystem.core.frontend.Services.Contracts
{
    public interface ICustomerService
    {
        // Khách hàng vãng lai (Public form)
        Task<ApiResponse<CustomerDto>> SubmitConsultationFormAsync(CustomerFormDto formDto);

        // Quản trị viên (Admin / Manager)
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<CustomerDto?> GetByIdAsync(string id);
        Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
        Task<CustomerDto> UpdateAsync(string id, UpdateCustomerDto dto);
        Task<bool> DeleteAsync(string id);
        Task<bool> ApproveCustomerAsync(string id);
        Task<bool> ToggleStatusAsync(string id);
        Task<IEnumerable<CustomerDto>> SearchAsync(string keyword);
        Task<IEnumerable<CustomerDto>> FilterAsync(string? address, int? birthYear);
        Task<byte[]> ExportExcelAsync(DateTime fromDate, DateTime toDate);
    }
}
