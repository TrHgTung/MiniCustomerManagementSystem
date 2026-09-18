using System.Net.Http.Json;
using CustomerManagementSystem.core.frontend.Models.Common;
using CustomerManagementSystem.core.frontend.Models.Customer;
using CustomerManagementSystem.core.frontend.Services.Contracts;

namespace CustomerManagementSystem.core.frontend.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly HttpClient _httpClient;

        public CustomerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<CustomerDto>> SubmitConsultationFormAsync(CustomerFormDto formDto)
        {
            var response = await _httpClient.PostAsJsonAsync("customer/form", formDto);
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = "Gửi thông tin không thành công. Vui lòng thử lại sau.";
                try
                {
                    var errorObj = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
                    if (!string.IsNullOrWhiteSpace(errorObj?.Message))
                    {
                        errorMessage = errorObj.Message;
                    }
                }
                catch
                {
                    var raw = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(raw)) errorMessage = raw;
                }
                throw new Exception(errorMessage);
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();
            return result ?? new ApiResponse<CustomerDto>();
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("admin/customers");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<CustomerDto>>() ?? Enumerable.Empty<CustomerDto>();
        }

        public async Task<CustomerDto?> GetByIdAsync(string id)
        {
            var response = await _httpClient.GetAsync($"admin/customers/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CustomerDto>();
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("admin/customers", dto);
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = "Tạo mới khách hàng thất bại.";
                try
                {
                    var errObj = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
                    if (!string.IsNullOrWhiteSpace(errObj?.Message)) errorMessage = errObj.Message;
                }
                catch
                {
                    var raw = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(raw)) errorMessage = raw;
                }
                throw new Exception(errorMessage);
            }
            return (await response.Content.ReadFromJsonAsync<CustomerDto>())!;
        }

        public async Task<string> UpdateAsync(string id, UpdateCustomerDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"admin/customers/{id}", dto);
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = "Cập nhật khách hàng thất bại.";
                try
                {
                    var errObj = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
                    if (!string.IsNullOrWhiteSpace(errObj?.Message)) errorMessage = errObj.Message;
                }
                catch
                {
                    var raw = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(raw)) errorMessage = raw;
                }
                throw new Exception(errorMessage);
            }
            try
            {
                var msgObj = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
                return msgObj?.Message ?? "Cập nhật thành công.";
            }
            catch
            {
                return "Cập nhật thành công.";
            }
        }

        public async Task<string> DeleteAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"admin/customers/{id}");
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = "Xóa khách hàng thất bại.";
                try
                {
                    var errObj = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
                    if (!string.IsNullOrWhiteSpace(errObj?.Message)) errorMessage = errObj.Message;
                }
                catch
                {
                    var raw = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(raw)) errorMessage = raw;
                }
                throw new Exception(errorMessage);
            }
            try
            {
                var msgObj = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
                return msgObj?.Message ?? "Xóa thành công.";
            }
            catch
            {
                return "Xóa thành công.";
            }
        }

        public async Task<string> ApproveCustomerAsync(string id)
        {
            var response = await _httpClient.PatchAsync($"admin/customers/{id}/approve", null);
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = "Duyệt khách hàng thất bại.";
                try
                {
                    var errObj = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
                    if (!string.IsNullOrWhiteSpace(errObj?.Message)) errorMessage = errObj.Message;
                }
                catch
                {
                    var raw = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(raw)) errorMessage = raw;
                }
                throw new Exception(errorMessage);
            }
            try
            {
                var msgObj = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
                return msgObj?.Message ?? "Duyệt khách hàng thành công.";
            }
            catch
            {
                return "Duyệt khách hàng thành công.";
            }
        }

        public async Task<bool> ToggleStatusAsync(string id)
        {
            var response = await _httpClient.PatchAsync($"admin/customers/{id}/toggle-status", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<CustomerDto>> SearchAsync(string keyword)
        {
            var encoded = Uri.EscapeDataString(keyword);
            var response = await _httpClient.GetAsync($"admin/customers/search?keyword={encoded}");
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = "Tìm kiếm không thành công.";
                try
                {
                    var errObj = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
                    if (!string.IsNullOrWhiteSpace(errObj?.Message)) errorMessage = errObj.Message;
                }
                catch
                {
                    var raw = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(raw)) errorMessage = raw;
                }
                throw new Exception(errorMessage);
            }
            return await response.Content.ReadFromJsonAsync<IEnumerable<CustomerDto>>() ?? Enumerable.Empty<CustomerDto>();
        }

        public async Task<IEnumerable<CustomerDto>> FilterAsync(string? address, int? birthYear)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(address))
                queryParams.Add($"address={Uri.EscapeDataString(address)}");
            if (birthYear.HasValue)
                queryParams.Add($"birthYear={birthYear.Value}");

            var query = string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"admin/customers/filter?{query}");
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = "Lọc dữ liệu không thành công.";
                try
                {
                    var errObj = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
                    if (!string.IsNullOrWhiteSpace(errObj?.Message)) errorMessage = errObj.Message;
                }
                catch
                {
                    var raw = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(raw)) errorMessage = raw;
                }
                throw new Exception(errorMessage);
            }
            return await response.Content.ReadFromJsonAsync<IEnumerable<CustomerDto>>() ?? Enumerable.Empty<CustomerDto>();
        }

        public async Task<IEnumerable<CustomerDto>> GetPendingDeletionAsync()
        {
            var response = await _httpClient.GetAsync("admin/customers/pending-deletion");
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = "Không thể tải danh sách chờ xóa.";
                try
                {
                    var errObj = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
                    if (!string.IsNullOrWhiteSpace(errObj?.Message)) errorMessage = errObj.Message;
                }
                catch
                {
                    var raw = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(raw)) errorMessage = raw;
                }
                throw new Exception(errorMessage);
            }
            return await response.Content.ReadFromJsonAsync<IEnumerable<CustomerDto>>() ?? Enumerable.Empty<CustomerDto>();
        }

        public async Task<byte[]> ExportExcelAsync(DateTime fromDate, DateTime toDate)
        {
            var from = fromDate.ToString("yyyy-MM-dd");
            var to = toDate.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync($"admin/export/customers?FromDate={from}&ToDate={to}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
