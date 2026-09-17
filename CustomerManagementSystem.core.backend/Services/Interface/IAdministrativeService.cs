using CustomerManagementSystem.core.backend.Data.DTO.Administrative;

namespace CustomerManagementSystem.core.backend.Services.Interface
{
    public interface IAdministrativeService
    {
        Task<IEnumerable<ManagerDto>> GetAllManagersAsync();
        Task<ManagerDto?> GetManagerByIdAsync(string id);
        Task<ManagerDto> CreateManagerAsync(CreateManagerDto createDto);
        Task<bool> DeactivateManagerAsync(string id);
    }
}
