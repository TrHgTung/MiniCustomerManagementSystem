using System.Net.Http.Json;
using CustomerManagementSystem.core.frontend.Models.Administrative;
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
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<ManagerDto>>() ?? Enumerable.Empty<ManagerDto>();
        }

        public async Task<ManagerDto?> GetManagerByIdAsync(string id)
        {
            var response = await _httpClient.GetAsync($"admin/managers/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ManagerDto>();
        }

        public async Task<ManagerDto> CreateManagerAsync(CreateManagerDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("admin/managers", dto);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<ManagerDto>())!;
        }

        public async Task<bool> ToggleManagerStatusAsync(string id)
        {
            var response = await _httpClient.PatchAsync($"admin/managers/{id}/toggle-status", null);
            return response.IsSuccessStatusCode;
        }
    }
}
