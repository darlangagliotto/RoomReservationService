using RoomService.Domain.Entities;

namespace RoomService.Domain.Repositories
{
    public interface IEquipmentRepository
    {
        Task<Equipment?> GetByIdAsync(Guid id);
        Task<Equipment?> GetByTypeAsync(string type);
        Task<Equipment?> GetByBrandAsync(string brand);
        Task AddSync(Equipment equipment);
        Task UpdateAsync(Equipment equipment);
        Task DeleteAsync(Equipment equipment);
    }
}