using RoomService.Domain.Entities;

namespace RoomService.Domain.Repositories
{
    public interface IRoomRepository
    {
        Task<List<Room>> GetAllAsync();
        Task<Room?> GetByIdAsync(Guid id);
        Task<Room?> GetByNameAsync(string name);
        Task<Room?> GetByNumberAsync(int number);
        Task<Room?> GetByNameOrNumberAsync(string name, int number);
        Task<bool> ExistsByEquipmentIdAsync(Guid equipmentId);
        Task AddSync(Room room);
        Task UpdateAsync(Room room);
        Task DeleteAsync(Room room);
    }
}