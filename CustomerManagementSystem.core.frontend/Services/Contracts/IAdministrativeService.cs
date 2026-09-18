using CustomerManagementSystem.core.frontend.Models.Administrative;

namespace CustomerManagementSystem.core.frontend.Services.Contracts
{
    public interface IAdministrativeService
    {
        Task<IEnumerable<ManagerDto>> GetAllManagersAsync();
        Task<ManagerDto?> GetManagerByIdAsync(string id);
        Task<ManagerDto> CreateManagerAsync(CreateManagerDto dto);
        Task<string> DeactivateManagerAsync(string id);
    }
}
