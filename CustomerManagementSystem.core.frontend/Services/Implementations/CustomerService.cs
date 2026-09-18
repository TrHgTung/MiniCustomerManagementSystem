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
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(string.IsNullOrWhiteSpace(error) ? "Gửi biểu mẫu thất bại." : error, null, response.StatusCode);
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
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<CustomerDto>())!;
        }

        public async Task<CustomerDto> UpdateAsync(string id, UpdateCustomerDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"admin/customers/{id}", dto);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<CustomerDto>())!;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"admin/customers/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ApproveCustomerAsync(string id)
        {
            var response = await _httpClient.PatchAsync($"admin/customers/{id}/approve", null);
            return response.IsSuccessStatusCode;
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
            response.EnsureSuccessStatusCode();
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
            response.EnsureSuccessStatusCode();
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
