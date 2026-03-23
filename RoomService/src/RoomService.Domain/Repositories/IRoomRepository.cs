using RoomService.Domain.Entities;

namespace RoomService.Domain.Repositories
{
    public interface IRoomRepository
    {
        Task<Room?> GetByIdAsync(Guid id);
        Task<Room?> GetByNameAsync(string name);
        Task<Room?> GetByNumberAsync(int number);
        Task AddSync(Room room);
        Task UpdateAsync(Room room);
        Task DeleteAsync(Room room);
    }
}