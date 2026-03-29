using RoomService.Domain.Entities;

namespace RoomService.Domain.Repositories
{
    public interface IEquipmentRepository
    {
        Task<Equipment?> GetByIdAsync(Guid id);
        Task<Equipment?> GetByTypeAsync(string equipmentType);
        Task<Equipment?> GetByBrandAsync(string brand);
        Task<Equipment?> GetBySerialNumberAsync(string serialNumber);
        Task AddSync(Equipment equipment);
        Task UpdateAsync(Equipment equipment);
        Task DeleteAsync(Equipment equipment);
    }
}