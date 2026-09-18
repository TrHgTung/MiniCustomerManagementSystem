using System.Net.Http.Json;
using CustomerManagementSystem.core.frontend.Models.Administrative;
using CustomerManagementSystem.core.frontend.Models.Common;
using CustomerManagementSystem.core.frontend.Services.Contracts;

namespace CustomerManagementSystem.core.frontend.Services.Implementations
{
    public class AdministrativeService : IAdministrativeService
    {
        private readonly HttpClient _httpClient;

        public AdministrativeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<ManagerDto>> GetAllManagersAsync()
        {
            var response = await _httpClient.GetAsync("admin/managers");
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = "Không thể tải danh sách tài khoản Manager.";
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
            return await response.Content.ReadFromJsonAsync<IEnumerable<ManagerDto>>() ?? Enumerable.Empty<ManagerDto>();
        }

        public async Task<ManagerDto?> GetManagerByIdAsync(string id)
        {
            var response = await _httpClient.GetAsync($"admin/managers/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = $"Không thể tải thông tin Manager '{id}'.";
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
            return await response.Content.ReadFromJsonAsync<ManagerDto>();
        }

        public async Task<ManagerDto> CreateManagerAsync(CreateManagerDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("admin/managers", dto);
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = "Tạo tài khoản Manager không thành công.";
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
            return (await response.Content.ReadFromJsonAsync<ManagerDto>())!;
        }

        public async Task<string> DeactivateManagerAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"admin/managers/{id}");
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = "Vô hiệu hóa tài khoản Manager không thành công.";
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
                return msgObj?.Message ?? "Đã vô hiệu hóa tài khoản Manager thành công.";
            }
            catch
            {
                return "Đã vô hiệu hóa tài khoản Manager thành công.";
            }
        }
    }
}
